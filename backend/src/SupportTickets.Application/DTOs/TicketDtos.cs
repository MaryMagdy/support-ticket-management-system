using SupportTickets.Domain.Enums;

namespace SupportTickets.Application.DTOs;

public record TicketDto(
    int Id,
    string Title,
    string Description,
    TicketStatus Status,
    TicketPriority Priority,
    int CustomerId,
    string? CustomerName,
    int? AssignedAgentId,
    string? AssignedAgentName,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int TotalTimeMinutes
);

public record CreateTicketRequest(string Title, string Description, TicketPriority Priority);

public record UpdateTicketRequest(
    string? Title,
    string? Description,
    TicketStatus? Status,
    TicketPriority? Priority,
    int? AssignedAgentId
);

public record TicketQueryParameters
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public TicketStatus? Status { get; init; }
    public TicketPriority? Priority { get; init; }
    public int? AssignedAgentId { get; init; }
    public string? Search { get; init; }
    public string? SortBy { get; init; } = "CreatedAt";
    public bool Descending { get; init; } = true;
}
