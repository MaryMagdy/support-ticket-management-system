using SupportTickets.Domain.Enums;

namespace SupportTickets.Application.DTOs;

public record CommentDto(int Id, int TicketId, int UserId, string UserName, string Text, DateTime CreatedAt);

public record CreateCommentRequest(string Text);

public record TimeEntryDto(int Id, int TicketId, int UserId, string UserName, DateTime WorkDate, int DurationMinutes, string? Description, DateTime CreatedAt);

public record CreateTimeEntryRequest(DateTime WorkDate, int DurationMinutes, string? Description);

public record ActivityLogDto(int Id, int TicketId, int UserId, string UserName, ActivityType Type, string? OldValue, string? NewValue, DateTime CreatedAt);

public record DashboardSummaryDto(
    int TotalTickets,
    Dictionary<string, int> CountsByStatus,
    int OpenAndCriticalCount,
    double? AverageResolutionTimeHours,
    List<AgentWorkloadDto> AgentWorkload
);

public record AgentWorkloadDto(int AgentId, string AgentName, int OpenTicketCount);
