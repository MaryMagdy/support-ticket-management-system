using Microsoft.EntityFrameworkCore;
using SupportTickets.Application.DTOs;
using SupportTickets.Application.Interfaces;
using SupportTickets.Domain.Enums;
using SupportTickets.Domain.Rules;
using SupportTickets.Infrastructure.Persistence;

namespace SupportTickets.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;

    public DashboardService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync()
    {
        var tickets = await _db.Tickets.Include(t => t.AssignedAgent).ToListAsync();

        var countsByStatus = Enum.GetValues<TicketStatus>()
            .ToDictionary(s => s.ToString(), s => tickets.Count(t => t.Status == s));

        // Tickets needing urgent attention: either still Open or of Critical priority (and not yet closed).
        var openAndCritical = tickets.Count(t =>
            t.Status != TicketStatus.Closed &&
            (t.Status == TicketStatus.Open || t.Priority == TicketPriority.Critical));

        var avgResolutionHours = TicketCalculations.AverageResolutionTimeHours(tickets);

        var agentWorkload = tickets
            .Where(t => t.AssignedAgentId.HasValue && t.Status != TicketStatus.Closed && t.Status != TicketStatus.Resolved)
            .GroupBy(t => new { t.AssignedAgentId, Name = t.AssignedAgent!.FullName })
            .Select(g => new AgentWorkloadDto(g.Key.AssignedAgentId!.Value, g.Key.Name, g.Count()))
            .OrderByDescending(a => a.OpenTicketCount)
            .ToList();

        return new DashboardSummaryDto(
            tickets.Count,
            countsByStatus,
            openAndCritical,
            avgResolutionHours,
            agentWorkload
        );
    }
}
