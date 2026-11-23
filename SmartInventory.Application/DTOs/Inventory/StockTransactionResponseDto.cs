
using SmartInventory.Core.Enums;

namespace SmartInventory.Application.DTOs.Inventory;

public class StockTransactionResponseDto
{
    public int StockTransactionId { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public int WarehouseId { get; set; }

    public string WarehouseName { get; set; } = string.Empty;

    public int? UserId { get; set; }

    public StockTransactionType TransactionType { get; set; }

    public decimal Quantity { get; set; }

    public string? ReferenceType { get; set; }

    public int? ReferenceId { get; set; }

    public string? Notes { get; set; }

    public DateTime TransactionDate { get; set; }
}
