
namespace SmartInventory.Application.DTOs.Warehouses;

public class WarehouseResponseDto
{
    public int WarehouseId { get; set; }

    public string WarehouseName { get; set; } = string.Empty;

    public string? Location { get; set; }

    public string? ContactNumber { get; set; }

    public bool IsActive { get; set; }
}
