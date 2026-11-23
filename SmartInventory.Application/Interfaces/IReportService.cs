
using SmartInventory.Application.DTOs.Reports;

namespace SmartInventory.Application.Interfaces;

public interface IReportService
{
    Task<List<SalesReportDto>> GetSalesReportAsync(DateTime? fromDate, DateTime? toDate);
    Task<List<PurchaseReportDto>> GetPurchaseReportAsync(DateTime? fromDate, DateTime? toDate);
    Task<List<InventoryReportDto>> GetInventoryReportAsync();
    Task<List<StockTransactionReportDto>> GetStockTransactionReportAsync(DateTime? fromDate, DateTime? toDate);
}
