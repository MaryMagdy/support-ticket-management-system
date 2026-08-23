using SupportTickets.Domain.Entities;
using SupportTickets.Domain.Enums;

namespace SupportTickets.Domain.Rules;

public static class TicketCalculations
{
    public static int TotalTimeMinutes(IEnumerable<TimeEntry> entries) => entries.Sum(e => e.DurationMinutes);

    /// <summary>
    /// Average resolution time (in hours) across resolved/closed tickets, computed as UpdatedAt - CreatedAt.
    /// Returns null when there are no qualifying tickets.
    /// </summary>
    public static double? AverageResolutionTimeHours(IEnumerable<Ticket> tickets)
    {
        var resolved = tickets
            .Where(t => t.Status == TicketStatus.Resolved || t.Status == TicketStatus.Closed)
            .ToList();

        if (resolved.Count == 0) return null;

        var totalHours = resolved.Sum(t => (t.UpdatedAt - t.CreatedAt).TotalHours);
        return totalHours / resolved.Count;
    }
}
