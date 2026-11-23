
namespace SmartInventory.Application.DTOs.StockTransfers;

public class StockTransferResponseDto
{
    public int StockTransferId { get; set; }
    public int SourceWarehouseId { get; set; }
    public string SourceWarehouseName { get; set; } = string.Empty;
    public int DestinationWarehouseId { get; set; }
    public string DestinationWarehouseName { get; set; } = string.Empty;
    public DateTime TransferDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public List<StockTransferItemResponseDto> Items { get; set; } = new();
}

public class StockTransferItemResponseDto
{
    public int StockTransferItemId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
}
