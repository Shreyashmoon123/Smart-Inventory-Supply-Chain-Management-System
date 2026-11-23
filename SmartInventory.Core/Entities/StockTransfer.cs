using SmartInventory.Core.Enums;

namespace SmartInventory.Core.Entities;

public class StockTransfer
{
    public int StockTransferId { get; set; }

    public int SourceWarehouseId { get; set; }

    public int DestinationWarehouseId { get; set; }

    public DateTime TransferDate { get; set; } = DateTime.UtcNow;

    public StockTransferStatus Status { get; set; } = StockTransferStatus.Pending;

    public string? Notes { get; set; }

    public Warehouse SourceWarehouse { get; set; } = null!;

    public Warehouse DestinationWarehouse { get; set; } = null!;

    public ICollection<StockTransferItem> Items { get; set; } = new List<StockTransferItem>();
}