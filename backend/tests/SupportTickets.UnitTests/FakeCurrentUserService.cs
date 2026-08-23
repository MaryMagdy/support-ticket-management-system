using SupportTickets.Application.Interfaces;
using SupportTickets.Domain.Enums;

namespace SupportTickets.UnitTests;

public class FakeCurrentUserService : ICurrentUserService
{
    public FakeCurrentUserService(int userId, UserRole role)
    {
        UserId = userId;
        Role = role;
    }

    public int UserId { get; }
    public UserRole Role { get; }
    public bool IsAuthenticated => true;
}
