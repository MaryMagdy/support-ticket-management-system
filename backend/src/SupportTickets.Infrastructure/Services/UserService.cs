using Microsoft.EntityFrameworkCore;
using SupportTickets.Application.Common;
using SupportTickets.Application.DTOs;
using SupportTickets.Application.Interfaces;
using SupportTickets.Domain.Entities;
using SupportTickets.Domain.Enums;
using SupportTickets.Infrastructure.Persistence;

namespace SupportTickets.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher _hasher;

    public UserService(AppDbContext db, IPasswordHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    public async Task<List<UserDto>> GetAllAsync(UserRole? role = null)
    {
        var query = _db.Users.AsQueryable();
        if (role.HasValue) query = query.Where(u => u.Role == role.Value);
        return await query
            .OrderBy(u => u.FullName)
            .Select(u => new UserDto(u.Id, u.Email, u.FullName, u.Role, u.CreatedAt))
            .ToListAsync();
    }

    public async Task<UserDto> GetByIdAsync(int id)
    {
        var user = await _db.Users.FindAsync(id) ?? throw new NotFoundException("User not found.");
        return new UserDto(user.Id, user.Email, user.FullName, user.Role, user.CreatedAt);
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request)
    {
        var existing = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (existing != null) throw new ConflictAppException("A user with this email already exists.");

        var user = new User
        {
            Email = request.Email,
            FullName = request.FullName,
            Role = request.Role,
            PasswordHash = _hasher.Hash(request.Password),
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return new UserDto(user.Id, user.Email, user.FullName, user.Role, user.CreatedAt);
    }

    public async Task<UserDto> UpdateAsync(int id, UpdateUserRequest request)
    {
        var user = await _db.Users.FindAsync(id) ?? throw new NotFoundException("User not found.");
        user.FullName = request.FullName;
        user.Role = request.Role;
        await _db.SaveChangesAsync();
        return new UserDto(user.Id, user.Email, user.FullName, user.Role, user.CreatedAt);
    }

    public async Task DeleteAsync(int id)
    {
        var user = await _db.Users.FindAsync(id) ?? throw new NotFoundException("User not found.");
        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
    }
}
