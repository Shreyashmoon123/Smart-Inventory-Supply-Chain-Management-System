
using SmartInventory.Application.DTOs.Dashboard;

namespace SmartInventory.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync();
    Task<List<LowStockProductDto>> GetLowStockAsync();
    Task<List<RecentTransactionDto>> GetRecentTransactionsAsync();
}
