using SupportTickets.Domain.Enums;

namespace SupportTickets.Domain.Entities;

public class Ticket
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;

    public int CustomerId { get; set; }
    public User? Customer { get; set; }

    public int? AssignedAgentId { get; set; }
    public User? AssignedAgent { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
    public ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();

    public int TotalTimeMinutes => TimeEntries?.Sum(t => t.DurationMinutes) ?? 0;

    /// <summary>
    /// App-managed concurrency token (a new value is assigned on every update). Modeled as a
    /// plain Guid rather than a DB-generated rowversion column so it works identically on both
    /// SQLite (dev) and SQL Server (prod) — SQLite has no native rowversion type.
    /// </summary>
    public Guid RowVersion { get; set; } = Guid.NewGuid();
}
