
namespace SmartInventory.Application.DTOs.Authentication;

public class AuthTokenDto
{
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
}
