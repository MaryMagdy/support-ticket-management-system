namespace SupportTickets.Domain.Enums;

public enum UserRole
{
    Admin = 0,
    SupportAgent = 1,
    Customer = 2
}

public enum TicketStatus
{
    Open = 0,
    InProgress = 1,
    Resolved = 2,
    Closed = 3
}

public enum TicketPriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

public enum ActivityType
{
    StatusChange = 0,
    PriorityChange = 1,
    AssignmentChange = 2,
    CommentAdded = 3
}
