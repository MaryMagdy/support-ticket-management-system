using Microsoft.EntityFrameworkCore;
using SupportTickets.Application.Common;
using SupportTickets.Application.DTOs;
using SupportTickets.Application.Interfaces;
using SupportTickets.Domain.Entities;
using SupportTickets.Domain.Enums;
using SupportTickets.Infrastructure.Persistence;

namespace SupportTickets.Infrastructure.Services;

public class TimeEntryService : ITimeEntryService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public TimeEntryService(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<List<TimeEntryDto>> GetByTicketAsync(int ticketId)
    {
        var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId) ?? throw new NotFoundException("Ticket not found.");
        EnforceAccess(ticket);

        return await _db.TimeEntries
            .Include(t => t.User)
            .Where(t => t.TicketId == ticketId)
            .OrderByDescending(t => t.WorkDate)
            .Select(t => new TimeEntryDto(t.Id, t.TicketId, t.UserId, t.User!.FullName, t.WorkDate, t.DurationMinutes, t.Description, t.CreatedAt))
            .ToListAsync();
    }

    public async Task<TimeEntryDto> AddAsync(int ticketId, CreateTimeEntryRequest request)
    {
        if (_currentUser.Role != UserRole.SupportAgent && _currentUser.Role != UserRole.Admin)
        {
            throw new ForbiddenException("Only support agents or admins can log time.");
        }

        var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId) ?? throw new NotFoundException("Ticket not found.");
        EnforceAccess(ticket);

        var entry = new TimeEntry
        {
            TicketId = ticketId,
            UserId = _currentUser.UserId,
            WorkDate = request.WorkDate,
            DurationMinutes = request.DurationMinutes,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow
        };

        _db.TimeEntries.Add(entry);
        await _db.SaveChangesAsync();

        var user = await _db.Users.FindAsync(_currentUser.UserId);

        return new TimeEntryDto(entry.Id, entry.TicketId, entry.UserId, user?.FullName ?? string.Empty, entry.WorkDate, entry.DurationMinutes, entry.Description, entry.CreatedAt);
    }

    private void EnforceAccess(Ticket ticket)
    {
        if (_currentUser.Role == UserRole.SupportAgent && ticket.AssignedAgentId != _currentUser.UserId)
        {
            throw new NotFoundException("Ticket not found.");
        }
    }
}
