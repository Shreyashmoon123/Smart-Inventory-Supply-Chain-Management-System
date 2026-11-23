
using Microsoft.EntityFrameworkCore;
using SmartInventory.Application.DTOs.Authentication;
using SmartInventory.Application.Interfaces;
using SmartInventory.Core.Entities;
using SmartInventory.Infrastructure.Data;

namespace SmartInventory.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IPasswordService _passwordService;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(
        AppDbContext context,
        IPasswordService passwordService,
        IJwtTokenService jwtTokenService)
    {
        _context = context;
        _passwordService = passwordService;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var emailExists = await _context.Users
            .AnyAsync(u => u.Email == email);

        if (emailExists)
        {
            throw new InvalidOperationException(
                "Email is already registered.");
        }

        var staffRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.RoleName == "Staff");

        if (staffRole == null)
        {
            throw new InvalidOperationException(
                "Default Staff role was not found.");
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = email,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = _passwordService.HashPassword(
            user,
            request.Password
        );

        user.UserRoles.Add(new UserRole
        {
            User = user,
            Role = staffRole,
            RoleId = staffRole.RoleId
        });

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        return CreateAuthResponse(
            user,
            new List<string> { staffRole.RoleName }
        );
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        var passwordValid = _passwordService.VerifyPassword(
            user,
            user.PasswordHash,
            request.Password
        );

        if (!passwordValid)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        var roles = user.UserRoles
            .Select(ur => ur.Role.RoleName)
            .ToList();

        return CreateAuthResponse(user, roles);
    }

    private AuthResponseDto CreateAuthResponse(
        User user,
        List<string> roles)
    {
        var token = _jwtTokenService.GenerateToken(user, roles);

        return new AuthResponseDto
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Token = token.Token,
            ExpiresAt = token.ExpiresAt,
            Roles = roles
        };
    }
}
