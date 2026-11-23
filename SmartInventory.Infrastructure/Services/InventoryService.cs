
using Microsoft.EntityFrameworkCore;
using SmartInventory.Application.DTOs.Inventory;
using SmartInventory.Application.Interfaces;
using SmartInventory.Core.Entities;
using SmartInventory.Core.Enums;
using SmartInventory.Infrastructure.Data;

namespace SmartInventory.Infrastructure.Services;

public class InventoryService : IInventoryService
{
    private readonly AppDbContext _context;

    public InventoryService(AppDbContext context)
    {
        _context = context;
    }

    // GET ALL INVENTORY
    public async Task<List<InventoryResponseDto>> GetAllAsync()
    {
        return await _context.Inventories
            .Select(i => new InventoryResponseDto
            {
                InventoryId = i.InventoryId,
                ProductId = i.ProductId,
                ProductName = i.Product.ProductName,
                SKU = i.Product.SKU,
                WarehouseId = i.WarehouseId,
                WarehouseName = i.Warehouse.WarehouseName,
                Quantity = i.Quantity,
                ReorderLevel = i.Product.ReorderLevel,
                IsLowStock = i.Quantity <= i.Product.ReorderLevel,
                LastUpdated = i.LastUpdated
            })
            .ToListAsync();
    }

    // GET INVENTORY BY ID
    public async Task<InventoryResponseDto?> GetByIdAsync(int id)
    {
        return await _context.Inventories
            .Where(i => i.InventoryId == id)
            .Select(i => new InventoryResponseDto
            {
                InventoryId = i.InventoryId,
                ProductId = i.ProductId,
                ProductName = i.Product.ProductName,
                SKU = i.Product.SKU,
                WarehouseId = i.WarehouseId,
                WarehouseName = i.Warehouse.WarehouseName,
                Quantity = i.Quantity,
                ReorderLevel = i.Product.ReorderLevel,
                IsLowStock = i.Quantity <= i.Product.ReorderLevel,
                LastUpdated = i.LastUpdated
            })
            .FirstOrDefaultAsync();
    }

    // GET INVENTORY BY PRODUCT
    public async Task<List<InventoryResponseDto>> GetByProductAsync(
        int productId)
    {
        return await _context.Inventories
            .Where(i => i.ProductId == productId)
            .Select(i => new InventoryResponseDto
            {
                InventoryId = i.InventoryId,
                ProductId = i.ProductId,
                ProductName = i.Product.ProductName,
                SKU = i.Product.SKU,
                WarehouseId = i.WarehouseId,
                WarehouseName = i.Warehouse.WarehouseName,
                Quantity = i.Quantity,
                ReorderLevel = i.Product.ReorderLevel,
                IsLowStock = i.Quantity <= i.Product.ReorderLevel,
                LastUpdated = i.LastUpdated
            })
            .ToListAsync();
    }

    // GET INVENTORY BY WAREHOUSE
    public async Task<List<InventoryResponseDto>> GetByWarehouseAsync(
        int warehouseId)
    {
        return await _context.Inventories
            .Where(i => i.WarehouseId == warehouseId)
            .Select(i => new InventoryResponseDto
            {
                InventoryId = i.InventoryId,
                ProductId = i.ProductId,
                ProductName = i.Product.ProductName,
                SKU = i.Product.SKU,
                WarehouseId = i.WarehouseId,
                WarehouseName = i.Warehouse.WarehouseName,
                Quantity = i.Quantity,
                ReorderLevel = i.Product.ReorderLevel,
                IsLowStock = i.Quantity <= i.Product.ReorderLevel,
                LastUpdated = i.LastUpdated
            })
            .ToListAsync();
    }

    // GET LOW STOCK INVENTORY
    public async Task<List<InventoryResponseDto>> GetLowStockAsync()
    {
        return await _context.Inventories
            .Where(i => i.Quantity <= i.Product.ReorderLevel)
            .Select(i => new InventoryResponseDto
            {
                InventoryId = i.InventoryId,
                ProductId = i.ProductId,
                ProductName = i.Product.ProductName,
                SKU = i.Product.SKU,
                WarehouseId = i.WarehouseId,
                WarehouseName = i.Warehouse.WarehouseName,
                Quantity = i.Quantity,
                ReorderLevel = i.Product.ReorderLevel,
                IsLowStock = true,
                LastUpdated = i.LastUpdated
            })
            .ToListAsync();
    }

    // GET STOCK TRANSACTION HISTORY
    public async Task<List<StockTransactionResponseDto>>
        GetTransactionsAsync(int? productId, int? warehouseId)
    {
        var query = _context.StockTransactions.AsQueryable();

        if (productId.HasValue)
        {
            query = query.Where(t => t.ProductId == productId.Value);
        }

        if (warehouseId.HasValue)
        {
            query = query.Where(t => t.WarehouseId == warehouseId.Value);
        }

        return await query
            .OrderByDescending(t => t.TransactionDate)
            .Select(t => new StockTransactionResponseDto
            {
                StockTransactionId = t.StockTransactionId,
                ProductId = t.ProductId,
                ProductName = t.Product.ProductName,
                WarehouseId = t.WarehouseId,
                WarehouseName = t.Warehouse.WarehouseName,
                UserId = t.UserId,
                TransactionType = t.TransactionType,
                Quantity = t.Quantity,
                ReferenceType = t.ReferenceType,
                ReferenceId = t.ReferenceId,
                Notes = t.Notes,
                TransactionDate = t.TransactionDate
            })
            .ToListAsync();
    }

    // STOCK IN
    public async Task<InventoryResponseDto> StockInAsync(
        StockInDto request,
        int? userId)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p =>
                p.ProductId == request.ProductId && p.IsActive);

        if (product == null)
        {
            throw new InvalidOperationException(
                "Product not found or inactive.");
        }

        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w =>
                w.WarehouseId == request.WarehouseId && w.IsActive);

        if (warehouse == null)
        {
            throw new InvalidOperationException(
                "Warehouse not found or inactive.");
        }

        var inventory = await _context.Inventories
            .FirstOrDefaultAsync(i =>
                i.ProductId == request.ProductId &&
                i.WarehouseId == request.WarehouseId);

        if (inventory == null)
        {
            inventory = new Inventory
            {
                ProductId = request.ProductId,
                WarehouseId = request.WarehouseId,
                Quantity = 0,
                LastUpdated = DateTime.UtcNow
            };

            _context.Inventories.Add(inventory);
        }

        inventory.Quantity += request.Quantity;
        inventory.LastUpdated = DateTime.UtcNow;

        AddTransaction(
            request.ProductId,
            request.WarehouseId,
            userId,
            StockTransactionType.StockIn,
            request.Quantity,
            request.ReferenceType,
            request.ReferenceId,
            request.Notes);
        await SaveInventoryChangesAsync();

        return await GetByIdAsync(inventory.InventoryId)
            ?? throw new InvalidOperationException(
                "Inventory could not be retrieved.");
    }

    // STOCK OUT
    public async Task<InventoryResponseDto> StockOutAsync(
        StockOutDto request,
        int? userId)
    {
        var inventory = await _context.Inventories
            .FirstOrDefaultAsync(i =>
                i.ProductId == request.ProductId &&
                i.WarehouseId == request.WarehouseId);

        if (inventory == null)
        {
            throw new InvalidOperationException(
                "Inventory record not found. Add stock first.");
        }

        if (!await _context.Products.AnyAsync(p =>
            p.ProductId == request.ProductId && p.IsActive))
        {
            throw new InvalidOperationException(
                "Product not found or inactive.");
        }

        if (!await _context.Warehouses.AnyAsync(w =>
            w.WarehouseId == request.WarehouseId && w.IsActive))
        {
            throw new InvalidOperationException(
                "Warehouse not found or inactive.");
        }

        if (inventory.Quantity < request.Quantity)
        {
            throw new InvalidOperationException(
                "Insufficient stock available.");
        }

        inventory.Quantity -= request.Quantity;
        inventory.LastUpdated = DateTime.UtcNow;

        AddTransaction(
            request.ProductId,
            request.WarehouseId,
            userId,
            StockTransactionType.StockOut,
            request.Quantity,
            request.ReferenceType,
            request.ReferenceId,
            request.Notes);

        await SaveInventoryChangesAsync();
        return await GetByIdAsync(inventory.InventoryId)
            ?? throw new InvalidOperationException(
                "Inventory could not be retrieved.");
    }

    // ADJUST STOCK
    public async Task<InventoryResponseDto> AdjustStockAsync(
        AdjustStockDto request,
        int? userId)
    {
        var inventory = await _context.Inventories
            .FirstOrDefaultAsync(i =>
                i.ProductId == request.ProductId &&
                i.WarehouseId == request.WarehouseId);

        if (inventory == null)
        {
            throw new InvalidOperationException(
                "Inventory record not found. Add stock first.");
        }

        decimal difference = request.NewQuantity - inventory.Quantity;

        if (difference == 0)
        {
            throw new InvalidOperationException(
                "New quantity is the same as current quantity.");
        }

        inventory.Quantity = request.NewQuantity;
        inventory.LastUpdated = DateTime.UtcNow;

        AddTransaction(
            request.ProductId,
            request.WarehouseId,
            userId,
            StockTransactionType.Adjustment,
            difference,
            "Adjustment",
            null,
            request.Notes);

        await SaveInventoryChangesAsync();
        return await GetByIdAsync(inventory.InventoryId)
            ?? throw new InvalidOperationException(
                "Inventory could not be retrieved.");
    }
    private async Task SaveInventoryChangesAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException(
                "Inventory was updated by another user. Please refresh and try again.");
        }
    }
    // COMMON TRANSACTION CREATION
    private void AddTransaction(
        int productId,
        int warehouseId,
        int? userId,
        StockTransactionType type,
        decimal quantity,
        string? referenceType,
        int? referenceId,
        string? notes)
    {
        var transaction = new StockTransaction
        {
            ProductId = productId,
            WarehouseId = warehouseId,
            UserId = userId,
            TransactionType = type,
            Quantity = quantity,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            Notes = notes,
            TransactionDate = DateTime.UtcNow
        };

        _context.StockTransactions.Add(transaction);
    }
}
