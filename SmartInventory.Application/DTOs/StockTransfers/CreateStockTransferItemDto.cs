
using System.ComponentModel.DataAnnotations;

namespace SmartInventory.Application.DTOs.StockTransfers;

public class CreateStockTransferItemDto
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(0.001, double.MaxValue)]
    public decimal Quantity { get; set; }
}
