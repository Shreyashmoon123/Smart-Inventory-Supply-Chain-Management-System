
namespace SmartInventory.Application.DTOs.Dashboard;

public class DashboardSummaryDto
{
    public int TotalProducts { get; set; }
    public int TotalCategories { get; set; }
    public int TotalSuppliers { get; set; }
    public int TotalWarehouses { get; set; }
    public int TotalCustomers { get; set; }
    public int LowStockItems { get; set; }
    public decimal TotalInventoryQuantity { get; set; }
    public int PendingPurchaseOrders { get; set; }
    public int PendingSalesOrders { get; set; }
    public decimal TotalSales { get; set; }
    public decimal TotalPurchases { get; set; }
}
