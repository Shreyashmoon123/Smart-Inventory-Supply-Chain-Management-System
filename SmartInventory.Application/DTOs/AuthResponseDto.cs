
namespace SmartInventory.Application.DTOs.Authentication;

public class AuthResponseDto
{
    public int UserId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public List<string> Roles { get; set; } = new();
}
