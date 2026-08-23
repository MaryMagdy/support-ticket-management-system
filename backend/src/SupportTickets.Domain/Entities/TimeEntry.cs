namespace SupportTickets.Domain.Entities;

public class TimeEntry
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public Ticket? Ticket { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public DateTime WorkDate { get; set; }
    public int DurationMinutes { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
