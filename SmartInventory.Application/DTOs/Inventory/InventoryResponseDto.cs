
namespace SmartInventory.Application.DTOs.Inventory;

public class InventoryResponseDto
{
    public int InventoryId { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string SKU { get; set; } = string.Empty;

    public int WarehouseId { get; set; }

    public string WarehouseName { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    public int ReorderLevel { get; set; }

    public bool IsLowStock { get; set; }

    public DateTime LastUpdated { get; set; }
}
