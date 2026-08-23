using SupportTickets.Domain.Enums;

namespace SupportTickets.Application.DTOs;

public record RegisterRequest(string Email, string Password, string FullName);

public record LoginRequest(string Email, string Password);

public record RefreshRequest(string RefreshToken);

public record AuthResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt, UserDto User);

public record UserDto(int Id, string Email, string FullName, UserRole Role, DateTime CreatedAt);

public record CreateUserRequest(string Email, string Password, string FullName, UserRole Role);

public record UpdateUserRequest(string FullName, UserRole Role);
