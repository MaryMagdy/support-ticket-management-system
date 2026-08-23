using SupportTickets.Application.Common;
using SupportTickets.Application.DTOs;
using SupportTickets.Domain.Entities;

namespace SupportTickets.Application.Interfaces;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string hash, string password);
}

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshTokenValue();
    int AccessTokenMinutes { get; }
}

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshAsync(string refreshToken);
}

public interface IUserService
{
    Task<List<UserDto>> GetAllAsync(Domain.Enums.UserRole? role = null);
    Task<UserDto> GetByIdAsync(int id);
    Task<UserDto> CreateAsync(CreateUserRequest request);
    Task<UserDto> UpdateAsync(int id, UpdateUserRequest request);
    Task DeleteAsync(int id);
}

public interface ITicketService
{
    Task<PagedResult<TicketDto>> GetAllAsync(TicketQueryParameters query);
    Task<TicketDto> GetByIdAsync(int id);
    Task<TicketDto> CreateAsync(CreateTicketRequest request);
    Task<TicketDto> UpdateAsync(int id, UpdateTicketRequest request);
    Task DeleteAsync(int id);
    Task<List<ActivityLogDto>> GetActivityAsync(int id);
}

public interface ICommentService
{
    Task<List<CommentDto>> GetByTicketAsync(int ticketId);
    Task<CommentDto> AddAsync(int ticketId, CreateCommentRequest request);
}

public interface ITimeEntryService
{
    Task<List<TimeEntryDto>> GetByTicketAsync(int ticketId);
    Task<TimeEntryDto> AddAsync(int ticketId, CreateTimeEntryRequest request);
}

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync();
}
