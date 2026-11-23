
using System.ComponentModel.DataAnnotations;

namespace SmartInventory.Application.DTOs.Inventory;

public class AdjustStockDto
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int WarehouseId { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal NewQuantity { get; set; }

    public string? Notes { get; set; }
}
