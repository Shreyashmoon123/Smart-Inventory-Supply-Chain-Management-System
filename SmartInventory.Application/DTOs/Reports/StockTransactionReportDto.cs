
namespace SmartInventory.Application.DTOs.Reports;

public class StockTransactionReportDto
{
    public int StockTransactionId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string? ReferenceType { get; set; }
    public int? ReferenceId { get; set; }
    public string? Notes { get; set; }
    public DateTime TransactionDate { get; set; }
}
