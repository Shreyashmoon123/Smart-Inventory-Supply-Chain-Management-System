
using System.ComponentModel.DataAnnotations;

namespace SmartInventory.Application.DTOs.AuditLogs;

public class CreateAuditLogDto
{
    [Required]
    [MaxLength(100)]
    public string Action { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? EntityName { get; set; }

    [MaxLength(100)]
    public string? EntityId { get; set; }

    [MaxLength(2000)]
    public string? Details { get; set; }
}
