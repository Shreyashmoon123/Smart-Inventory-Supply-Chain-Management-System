
using Microsoft.EntityFrameworkCore;
using SmartInventory.Application.DTOs.Dashboard;
using SmartInventory.Application.Interfaces;
using SmartInventory.Infrastructure.Data;

namespace SmartInventory.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync()
    {
        return new DashboardSummaryDto
        {
            TotalProducts = await _context.Products.CountAsync(p => p.IsActive),
            TotalCategories = await _context.Categories.CountAsync(c => c.IsActive),
            TotalSuppliers = await _context.Suppliers.CountAsync(s => s.IsActive),
            TotalWarehouses = await _context.Warehouses.CountAsync(w => w.IsActive),
            TotalCustomers = await _context.Customers.CountAsync(),

            LowStockItems = await _context.Inventories
                .CountAsync(i => i.Quantity <= i.Product.ReorderLevel),

            TotalInventoryQuantity = await _context.Inventories
                .SumAsync(i => (decimal?)i.Quantity) ?? 0,

            PendingPurchaseOrders = await _context.PurchaseOrders
                .CountAsync(p => p.Status == Core.Enums.PurchaseOrderStatus.Pending),

            PendingSalesOrders = await _context.SalesOrders
                .CountAsync(s => s.Status == Core.Enums.SalesOrderStatus.Pending),

            TotalSales = await _context.SalesOrders
                .Where(s => s.Status != Core.Enums.SalesOrderStatus.Cancelled)
                .SumAsync(s => (decimal?)s.TotalAmount) ?? 0,

            TotalPurchases = await _context.PurchaseOrders
                .Where(p => p.Status != Core.Enums.PurchaseOrderStatus.Cancelled)
                .SumAsync(p => (decimal?)p.TotalAmount) ?? 0
        };
    }

    public async Task<List<LowStockProductDto>> GetLowStockAsync()
    {
        return await _context.Inventories
            .AsNoTracking()
            .Where(i => i.Quantity <= i.Product.ReorderLevel)
            .Select(i => new LowStockProductDto
            {
                ProductId = i.ProductId,
                ProductName = i.Product.ProductName,
                SKU = i.Product.SKU,
                WarehouseName = i.Warehouse.WarehouseName,
                Quantity = i.Quantity,
                ReorderLevel = i.Product.ReorderLevel
            })
            .OrderBy(i => i.Quantity)
            .ToListAsync();
    }

    public async Task<List<RecentTransactionDto>> GetRecentTransactionsAsync()
    {
        return await _context.StockTransactions
            .AsNoTracking()
            .OrderByDescending(t => t.TransactionDate)
            .Take(10)
            .Select(t => new RecentTransactionDto
            {
                StockTransactionId = t.StockTransactionId,
                ProductName = t.Product.ProductName,
                WarehouseName = t.Warehouse.WarehouseName,
                TransactionType = t.TransactionType.ToString(),
                Quantity = t.Quantity,
                Notes = t.Notes,
                TransactionDate = t.TransactionDate
            })
            .ToListAsync();
    }
}
