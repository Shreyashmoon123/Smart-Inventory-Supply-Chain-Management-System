
namespace SmartInventory.Application.DTOs.AuditLogs;

public class AuditLogResponseDto
{
    public int AuditLogId { get; set; }
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? EntityName { get; set; }
    public string? EntityId { get; set; }
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; }
}
