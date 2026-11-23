
using System.ComponentModel.DataAnnotations;

namespace SmartInventory.Application.DTOs.Inventory;

public class StockOutDto
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int WarehouseId { get; set; }

    [Range(typeof(decimal), "0.001", "999999999")]
    public decimal Quantity { get; set; }

    public string? ReferenceType { get; set; }

    public int? ReferenceId { get; set; }

    public string? Notes { get; set; }
}
