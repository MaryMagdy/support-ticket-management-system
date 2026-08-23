using Microsoft.EntityFrameworkCore;
using SupportTickets.Application.Common;
using SupportTickets.Application.DTOs;
using SupportTickets.Application.Interfaces;
using SupportTickets.Domain.Entities;
using SupportTickets.Domain.Enums;
using SupportTickets.Infrastructure.Persistence;

namespace SupportTickets.Infrastructure.Services;

public class CommentService : ICommentService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CommentService(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<List<CommentDto>> GetByTicketAsync(int ticketId)
    {
        var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId) ?? throw new NotFoundException("Ticket not found.");
        EnforceAccess(ticket);

        return await _db.Comments
            .Include(c => c.User)
            .Where(c => c.TicketId == ticketId)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new CommentDto(c.Id, c.TicketId, c.UserId, c.User!.FullName, c.Text, c.CreatedAt))
            .ToListAsync();
    }

    public async Task<CommentDto> AddAsync(int ticketId, CreateCommentRequest request)
    {
        var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId) ?? throw new NotFoundException("Ticket not found.");
        EnforceAccess(ticket);

        var comment = new Comment
        {
            TicketId = ticketId,
            UserId = _currentUser.UserId,
            Text = request.Text,
            CreatedAt = DateTime.UtcNow
        };

        _db.Comments.Add(comment);

        _db.ActivityLogs.Add(new ActivityLog
        {
            TicketId = ticketId,
            UserId = _currentUser.UserId,
            Type = ActivityType.CommentAdded,
            OldValue = null,
            NewValue = request.Text.Length > 100 ? request.Text[..100] : request.Text,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        var user = await _db.Users.FindAsync(_currentUser.UserId);

        return new CommentDto(comment.Id, comment.TicketId, comment.UserId, user?.FullName ?? string.Empty, comment.Text, comment.CreatedAt);
    }

    private void EnforceAccess(Ticket ticket)
    {
        switch (_currentUser.Role)
        {
            case UserRole.Customer when ticket.CustomerId != _currentUser.UserId:
                throw new NotFoundException("Ticket not found.");
            case UserRole.SupportAgent when ticket.AssignedAgentId != _currentUser.UserId:
                throw new NotFoundException("Ticket not found.");
        }
    }
}
