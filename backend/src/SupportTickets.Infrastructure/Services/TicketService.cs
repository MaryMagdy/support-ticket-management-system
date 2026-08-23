using Microsoft.EntityFrameworkCore;
using SupportTickets.Application.Common;
using SupportTickets.Application.DTOs;
using SupportTickets.Application.Interfaces;
using SupportTickets.Domain.Entities;
using SupportTickets.Domain.Enums;
using SupportTickets.Domain.Rules;
using SupportTickets.Infrastructure.Persistence;

namespace SupportTickets.Infrastructure.Services;

public class TicketService : ITicketService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public TicketService(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<TicketDto>> GetAllAsync(TicketQueryParameters query)
    {
        var q = _db.Tickets.Include(t => t.Customer).Include(t => t.AssignedAgent).Include(t => t.TimeEntries).AsQueryable();

        // Data isolation
        if (_currentUser.Role == UserRole.Customer)
        {
            q = q.Where(t => t.CustomerId == _currentUser.UserId);
        }
        else if (_currentUser.Role == UserRole.SupportAgent)
        {
            q = q.Where(t => t.AssignedAgentId == _currentUser.UserId);
        }
        // Admin: no restriction

        if (query.Status.HasValue) q = q.Where(t => t.Status == query.Status.Value);
        if (query.Priority.HasValue) q = q.Where(t => t.Priority == query.Priority.Value);
        if (query.AssignedAgentId.HasValue) q = q.Where(t => t.AssignedAgentId == query.AssignedAgentId.Value);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            q = q.Where(t => t.Title.Contains(term) || t.Description.Contains(term));
        }

        q = ApplySort(q, query.SortBy, query.Descending);

        var totalCount = await q.CountAsync();
        var page = Math.Max(query.Page, 1);
        var pageSize = query.PageSize <= 0 ? 20 : Math.Min(query.PageSize, 100);

        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<TicketDto>
        {
            Items = items.Select(ToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    private static IQueryable<Ticket> ApplySort(IQueryable<Ticket> q, string? sortBy, bool desc)
    {
        Func<IQueryable<Ticket>, IOrderedQueryable<Ticket>> orderFn = sortBy?.ToLowerInvariant() switch
        {
            "title" => desc ? (x => x.OrderByDescending(t => t.Title)) : (x => x.OrderBy(t => t.Title)),
            "priority" => desc ? (x => x.OrderByDescending(t => t.Priority)) : (x => x.OrderBy(t => t.Priority)),
            "status" => desc ? (x => x.OrderByDescending(t => t.Status)) : (x => x.OrderBy(t => t.Status)),
            "updatedat" => desc ? (x => x.OrderByDescending(t => t.UpdatedAt)) : (x => x.OrderBy(t => t.UpdatedAt)),
            _ => desc ? (x => x.OrderByDescending(t => t.CreatedAt)) : (x => x.OrderBy(t => t.CreatedAt)),
        };
        return orderFn(q);
    }

    public async Task<TicketDto> GetByIdAsync(int id)
    {
        var ticket = await _db.Tickets
            .Include(t => t.Customer)
            .Include(t => t.AssignedAgent)
            .Include(t => t.TimeEntries)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket == null) throw new NotFoundException("Ticket not found.");

        EnforceReadAccess(ticket);

        return ToDto(ticket);
    }

    public async Task<TicketDto> CreateAsync(CreateTicketRequest request)
    {
        if (_currentUser.Role != UserRole.Customer && _currentUser.Role != UserRole.Admin)
        {
            throw new ForbiddenException("Only customers or admins can create tickets.");
        }

        var ticket = new Ticket
        {
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            Status = TicketStatus.Open,
            CustomerId = _currentUser.UserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Tickets.Add(ticket);
        await _db.SaveChangesAsync();

        await _db.Entry(ticket).Reference(t => t.Customer).LoadAsync();

        return ToDto(ticket);
    }

    public async Task<TicketDto> UpdateAsync(int id, UpdateTicketRequest request)
    {
        var ticket = await _db.Tickets
            .Include(t => t.Customer)
            .Include(t => t.AssignedAgent)
            .Include(t => t.TimeEntries)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket == null) throw new NotFoundException("Ticket not found.");

        EnforceReadAccess(ticket);

        if (request.RowVersion.HasValue)
        {
            _db.Entry(ticket).Property(t => t.RowVersion).OriginalValue = request.RowVersion.Value;
        }

        // Title / description editable by owner (customer) or admin
        if (request.Title != null || request.Description != null)
        {
            if (_currentUser.Role == UserRole.SupportAgent)
            {
                throw new ForbiddenException("Support agents cannot edit ticket title/description.");
            }
            if (request.Title != null) ticket.Title = request.Title;
            if (request.Description != null) ticket.Description = request.Description;
        }

        if (request.Priority.HasValue && request.Priority.Value != ticket.Priority)
        {
            if (_currentUser.Role == UserRole.Customer)
            {
                throw new ForbiddenException("Customers cannot change ticket priority.");
            }
            var oldPriority = ticket.Priority;
            ticket.Priority = request.Priority.Value;
            LogActivity(ticket.Id, ActivityType.PriorityChange, oldPriority.ToString(), ticket.Priority.ToString());
        }

        if (request.AssignedAgentId.HasValue && request.AssignedAgentId.Value != ticket.AssignedAgentId)
        {
            if (_currentUser.Role != UserRole.Admin)
            {
                throw new ForbiddenException("Only admins can (re)assign tickets.");
            }
            var oldAgent = ticket.AssignedAgentId;
            ticket.AssignedAgentId = request.AssignedAgentId.Value;
            LogActivity(ticket.Id, ActivityType.AssignmentChange, oldAgent?.ToString(), ticket.AssignedAgentId?.ToString());
        }

        if (request.Status.HasValue && request.Status.Value != ticket.Status)
        {
            if (!TicketStatusRules.IsValidTransition(ticket.Status, request.Status.Value, _currentUser.Role))
            {
                throw new ValidationAppException($"Invalid status transition from {ticket.Status} to {request.Status.Value} for role {_currentUser.Role}.");
            }
            var oldStatus = ticket.Status;
            ticket.Status = request.Status.Value;
            LogActivity(ticket.Id, ActivityType.StatusChange, oldStatus.ToString(), ticket.Status.ToString());
        }

        ticket.UpdatedAt = DateTime.UtcNow;
        ticket.RowVersion = Guid.NewGuid();

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictAppException(
                "This ticket was modified by someone else since you loaded it. Refresh and try again.");
        }

        return ToDto(ticket);
    }

    public async Task DeleteAsync(int id)
    {
        if (_currentUser.Role != UserRole.Admin)
        {
            throw new ForbiddenException("Only admins can delete tickets.");
        }

        var ticket = await _db.Tickets.FindAsync(id) ?? throw new NotFoundException("Ticket not found.");
        _db.Tickets.Remove(ticket);
        await _db.SaveChangesAsync();
    }

    public async Task<List<ActivityLogDto>> GetActivityAsync(int id)
    {
        var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == id) ?? throw new NotFoundException("Ticket not found.");
        EnforceReadAccess(ticket);

        return await _db.ActivityLogs
            .Include(a => a.User)
            .Where(a => a.TicketId == id)
            .OrderBy(a => a.CreatedAt)
            .Select(a => new ActivityLogDto(a.Id, a.TicketId, a.UserId, a.User!.FullName, a.Type, a.OldValue, a.NewValue, a.CreatedAt))
            .ToListAsync();
    }

    private void EnforceReadAccess(Ticket ticket)
    {
        switch (_currentUser.Role)
        {
            case UserRole.Customer when ticket.CustomerId != _currentUser.UserId:
                throw new NotFoundException("Ticket not found.");
            case UserRole.SupportAgent when ticket.AssignedAgentId != _currentUser.UserId:
                throw new NotFoundException("Ticket not found.");
        }
    }

    private void LogActivity(int ticketId, ActivityType type, string? oldValue, string? newValue)
    {
        _db.ActivityLogs.Add(new ActivityLog
        {
            TicketId = ticketId,
            UserId = _currentUser.UserId,
            Type = type,
            OldValue = oldValue,
            NewValue = newValue,
            CreatedAt = DateTime.UtcNow
        });
    }

    private static TicketDto ToDto(Ticket t) => new(
        t.Id,
        t.Title,
        t.Description,
        t.Status,
        t.Priority,
        t.CustomerId,
        t.Customer?.FullName,
        t.AssignedAgentId,
        t.AssignedAgent?.FullName,
        t.CreatedAt,
        t.UpdatedAt,
        TicketCalculations.TotalTimeMinutes(t.TimeEntries),
        t.RowVersion
    );
}
