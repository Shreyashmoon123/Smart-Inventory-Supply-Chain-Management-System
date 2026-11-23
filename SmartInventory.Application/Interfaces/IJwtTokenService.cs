
using SmartInventory.Application.DTOs.Authentication;
using SmartInventory.Core.Entities;

namespace SmartInventory.Application.Interfaces;

public interface IJwtTokenService
{
    AuthTokenDto GenerateToken(User user, IEnumerable<string> roles);
}
