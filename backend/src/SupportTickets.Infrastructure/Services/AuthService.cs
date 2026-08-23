using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SupportTickets.Application.Common;
using SupportTickets.Application.DTOs;
using SupportTickets.Application.Interfaces;
using SupportTickets.Domain.Entities;
using SupportTickets.Domain.Enums;
using SupportTickets.Infrastructure.Identity;
using SupportTickets.Infrastructure.Persistence;

namespace SupportTickets.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokenService;
    private readonly JwtSettings _jwtSettings;

    public AuthService(AppDbContext db, IPasswordHasher hasher, ITokenService tokenService, IOptions<JwtSettings> jwtSettings)
    {
        _db = db;
        _hasher = hasher;
        _tokenService = tokenService;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var existing = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (existing != null)
        {
            throw new ConflictAppException("A user with this email already exists.");
        }

        var user = new User
        {
            Email = request.Email,
            FullName = request.FullName,
            Role = UserRole.Customer,
            PasswordHash = _hasher.Hash(request.Password),
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return await IssueTokensAsync(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null || !_hasher.Verify(user.PasswordHash, request.Password))
        {
            throw new UnauthorizedAppException("Invalid email or password.");
        }

        return await IssueTokensAsync(user);
    }

    public async Task<AuthResponse> RefreshAsync(string refreshToken)
    {
        var token = await _db.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == refreshToken);

        if (token == null || !token.IsActive || token.User == null)
        {
            throw new UnauthorizedAppException("Invalid or expired refresh token.");
        }

        // Rotate: revoke old, issue new
        token.RevokedAt = DateTime.UtcNow;

        var newRefreshTokenValue = _tokenService.GenerateRefreshTokenValue();
        token.ReplacedByToken = newRefreshTokenValue;

        var newRefreshToken = new RefreshToken
        {
            UserId = token.UserId,
            Token = newRefreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDays),
            CreatedAt = DateTime.UtcNow
        };

        _db.RefreshTokens.Add(newRefreshToken);
        await _db.SaveChangesAsync();

        var accessToken = _tokenService.GenerateAccessToken(token.User);

        return new AuthResponse(
            accessToken,
            newRefreshTokenValue,
            DateTime.UtcNow.AddMinutes(_tokenService.AccessTokenMinutes),
            new UserDto(token.User.Id, token.User.Email, token.User.FullName, token.User.Role, token.User.CreatedAt)
        );
    }

    private async Task<AuthResponse> IssueTokensAsync(User user)
    {
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshTokenValue = _tokenService.GenerateRefreshTokenValue();

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDays),
            CreatedAt = DateTime.UtcNow
        };

        _db.RefreshTokens.Add(refreshToken);
        await _db.SaveChangesAsync();

        return new AuthResponse(
            accessToken,
            refreshTokenValue,
            DateTime.UtcNow.AddMinutes(_tokenService.AccessTokenMinutes),
            new UserDto(user.Id, user.Email, user.FullName, user.Role, user.CreatedAt)
        );
    }
}
