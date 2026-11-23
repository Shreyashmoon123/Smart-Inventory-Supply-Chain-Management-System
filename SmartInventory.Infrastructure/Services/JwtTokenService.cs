
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SmartInventory.Application.DTOs.Authentication;
using SmartInventory.Application.Interfaces;
using SmartInventory.Core.Entities;

namespace SmartInventory.Infrastructure.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public AuthTokenDto GenerateToken(
        User user,
        IEnumerable<string> roles)
    {
        var key = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "JWT Key is missing.");

        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];

        if (Encoding.UTF8.GetByteCount(key) < 32)
        {
            throw new InvalidOperationException(
                "JWT Key must contain at least 32 bytes.");
        }

        if (string.IsNullOrWhiteSpace(issuer) ||
            string.IsNullOrWhiteSpace(audience))
        {
            throw new InvalidOperationException(
                "JWT Issuer and Audience must be configured.");
        }

        if (!int.TryParse(
            _configuration["Jwt:ExpirationInMinutes"],
            out var expirationMinutes) ||
            expirationMinutes < 1 ||
            expirationMinutes > 1440)
        {
            throw new InvalidOperationException(
                "JWT expiration must be between 1 and 1440 minutes.");
        }

        var expiresAt = DateTime.UtcNow.AddMinutes(
            expirationMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Name, user.FullName),
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.Email)
        };

        foreach (var role in roles.Distinct())
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(key));

        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        return new AuthTokenDto
        {
            Token = new JwtSecurityTokenHandler()
                .WriteToken(token),
            ExpiresAt = expiresAt
        };
    }
}
