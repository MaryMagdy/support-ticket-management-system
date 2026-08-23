using SupportTickets.Domain.Enums;

namespace SupportTickets.Application.Interfaces;

public interface ICurrentUserService
{
    int UserId { get; }
    UserRole Role { get; }
    bool IsAuthenticated { get; }
}
