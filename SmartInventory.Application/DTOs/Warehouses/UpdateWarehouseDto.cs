
using System.ComponentModel.DataAnnotations;

namespace SmartInventory.Application.DTOs.Warehouses;

public class UpdateWarehouseDto
{
    [Required]
    [MaxLength(150)]
    public string WarehouseName { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Location { get; set; }

    [MaxLength(20)]
    public string? ContactNumber { get; set; }

    public bool IsActive { get; set; }
}
