
using Microsoft.AspNetCore.Identity;
using SmartInventory.Application.Interfaces;
using SmartInventory.Core.Entities;

namespace SmartInventory.Infrastructure.Services;

public class PasswordHashingService : IPasswordService
{
    private readonly IPasswordHasher<User> _passwordHasher;

    public PasswordHashingService()
    {
        _passwordHasher = new PasswordHasher<User>();
    }

    public string HashPassword(User user, string password)
    {
        return _passwordHasher.HashPassword(user, password);
    }

    public bool VerifyPassword(
        User user,
        string hashedPassword,
        string providedPassword)
    {
        var result = _passwordHasher.VerifyHashedPassword(
            user,
            hashedPassword,
            providedPassword
        );

        return result != PasswordVerificationResult.Failed;
    }
}
