
namespace SmartInventory.Application.DTOs.Reports;

public class InventoryReportDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public int ReorderLevel { get; set; }
    public bool IsLowStock { get; set; }
}
