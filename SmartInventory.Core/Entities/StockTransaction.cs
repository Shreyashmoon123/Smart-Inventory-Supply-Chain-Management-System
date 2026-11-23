using SmartInventory.Core.Enums;

namespace SmartInventory.Core.Entities;

public class StockTransaction
{
    public int StockTransactionId { get; set; }

    public int ProductId { get; set; }

    public int WarehouseId { get; set; }

    public int? UserId { get; set; }

    public StockTransactionType TransactionType { get; set; }

    public decimal Quantity { get; set; }

    public string? ReferenceType { get; set; }

    public int? ReferenceId { get; set; }

    public string? Notes { get; set; }

    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

    public Product Product { get; set; } = null!;

    public Warehouse Warehouse { get; set; } = null!;

    public User? User { get; set; }
}