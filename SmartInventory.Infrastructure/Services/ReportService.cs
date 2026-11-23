
using Microsoft.EntityFrameworkCore;
using SmartInventory.Application.DTOs.Reports;
using SmartInventory.Application.Interfaces;
using SmartInventory.Infrastructure.Data;

namespace SmartInventory.Infrastructure.Services;

public class ReportService : IReportService
{
    private readonly AppDbContext _context;

    public ReportService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<SalesReportDto>> GetSalesReportAsync(
        DateTime? fromDate, DateTime? toDate)
    {
        var query = _context.SalesOrders
            .AsNoTracking()
            .Include(s => s.Customer)
            .AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(s => s.OrderDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(s => s.OrderDate < toDate.Value.Date.AddDays(1));

        return await query
            .OrderByDescending(s => s.OrderDate)
            .Select(s => new SalesReportDto
            {
                SalesOrderId = s.SalesOrderId,
                CustomerName = s.Customer.CustomerName,
                OrderDate = s.OrderDate,
                Status = s.Status.ToString(),
                TotalAmount = s.TotalAmount
            })
            .ToListAsync();
    }

    public async Task<List<PurchaseReportDto>> GetPurchaseReportAsync(
        DateTime? fromDate, DateTime? toDate)
    {
        var query = _context.PurchaseOrders
            .AsNoTracking()
            .Include(p => p.Supplier)
            .Include(p => p.Warehouse)
            .AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(p => p.OrderDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(p => p.OrderDate < toDate.Value.Date.AddDays(1));

        return await query
            .OrderByDescending(p => p.OrderDate)
            .Select(p => new PurchaseReportDto
            {
                PurchaseOrderId = p.PurchaseOrderId,
                SupplierName = p.Supplier.SupplierName,
                WarehouseName = p.Warehouse.WarehouseName,
                OrderDate = p.OrderDate,
                Status = p.Status.ToString(),
                TotalAmount = p.TotalAmount
            })
            .ToListAsync();
    }

    public async Task<List<InventoryReportDto>> GetInventoryReportAsync()
    {
        return await _context.Inventories
            .AsNoTracking()
            .Select(i => new InventoryReportDto
            {
                ProductId = i.ProductId,
                ProductName = i.Product.ProductName,
                SKU = i.Product.SKU,
                WarehouseName = i.Warehouse.WarehouseName,
                Quantity = i.Quantity,
                ReorderLevel = i.Product.ReorderLevel,
                IsLowStock = i.Quantity <= i.Product.ReorderLevel
            })
            .OrderBy(i => i.ProductName)
            .ToListAsync();
    }

    public async Task<List<StockTransactionReportDto>> GetStockTransactionReportAsync(
        DateTime? fromDate, DateTime? toDate)
    {
        var query = _context.StockTransactions
            .AsNoTracking()
            .AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(t => t.TransactionDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(t => t.TransactionDate < toDate.Value.Date.AddDays(1));

        return await query
            .OrderByDescending(t => t.TransactionDate)
            .Select(t => new StockTransactionReportDto
            {
                StockTransactionId = t.StockTransactionId,
                ProductName = t.Product.ProductName,
                WarehouseName = t.Warehouse.WarehouseName,
                TransactionType = t.TransactionType.ToString(),
                Quantity = t.Quantity,
                ReferenceType = t.ReferenceType,
                ReferenceId = t.ReferenceId,
                Notes = t.Notes,
                TransactionDate = t.TransactionDate
            })
            .ToListAsync();
    }
}
