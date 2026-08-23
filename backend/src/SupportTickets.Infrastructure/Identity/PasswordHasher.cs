using Microsoft.AspNetCore.Identity;
using SupportTickets.Application.Interfaces;
using SupportTickets.Domain.Entities;

namespace SupportTickets.Infrastructure.Identity;

public class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<User> _inner = new();

    public string Hash(string password) => _inner.HashPassword(null!, password);

    public bool Verify(string hash, string password)
    {
        var result = _inner.VerifyHashedPassword(null!, hash, password);
        return result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded;
    }
}
