
using Microsoft.EntityFrameworkCore;
using SmartInventory.Application.DTOs.Warehouses;
using SmartInventory.Application.Interfaces;
using SmartInventory.Core.Entities;
using SmartInventory.Infrastructure.Data;

namespace SmartInventory.Infrastructure.Services;

public class WarehouseService : IWarehouseService
{
    private readonly AppDbContext _context;

    public WarehouseService(AppDbContext context)
    {
        _context = context;
    }

    // GET ALL WAREHOUSES
    public async Task<List<WarehouseResponseDto>> GetAllAsync()
    {
        return await _context.Warehouses
            .Select(w => new WarehouseResponseDto
            {
                WarehouseId = w.WarehouseId,
                WarehouseName = w.WarehouseName,
                Location = w.Location,
                ContactNumber = w.ContactNumber,
                IsActive = w.IsActive
            })
            .ToListAsync();
    }

    // GET WAREHOUSE BY ID
    public async Task<WarehouseResponseDto?> GetByIdAsync(int id)
    {
        return await _context.Warehouses
            .Where(w => w.WarehouseId == id)
            .Select(w => new WarehouseResponseDto
            {
                WarehouseId = w.WarehouseId,
                WarehouseName = w.WarehouseName,
                Location = w.Location,
                ContactNumber = w.ContactNumber,
                IsActive = w.IsActive
            })
            .FirstOrDefaultAsync();
    }

    // CREATE WAREHOUSE
    public async Task<WarehouseResponseDto> CreateAsync(
        CreateWarehouseDto request)
    {
        string warehouseName = request.WarehouseName.Trim();

        bool exists = await _context.Warehouses
            .AnyAsync(w => w.WarehouseName == warehouseName);

        if (exists)
        {
            throw new InvalidOperationException(
                "Warehouse name already exists.");
        }

        var warehouse = new Warehouse
        {
            WarehouseName = warehouseName,
            Location = request.Location?.Trim(),
            ContactNumber = request.ContactNumber?.Trim(),
            IsActive = true
        };

        _context.Warehouses.Add(warehouse);

        await _context.SaveChangesAsync();

        return new WarehouseResponseDto
        {
            WarehouseId = warehouse.WarehouseId,
            WarehouseName = warehouse.WarehouseName,
            Location = warehouse.Location,
            ContactNumber = warehouse.ContactNumber,
            IsActive = warehouse.IsActive
        };
    }

    // UPDATE WAREHOUSE
    public async Task<WarehouseResponseDto?> UpdateAsync(
        int id,
        UpdateWarehouseDto request)
    {
        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.WarehouseId == id);

        if (warehouse == null)
        {
            return null;
        }

        string warehouseName = request.WarehouseName.Trim();

        bool duplicate = await _context.Warehouses
            .AnyAsync(w =>
                w.WarehouseName == warehouseName &&
                w.WarehouseId != id);

        if (duplicate)
        {
            throw new InvalidOperationException(
                "Warehouse name already exists.");
        }

        warehouse.WarehouseName = warehouseName;
        warehouse.Location = request.Location?.Trim();
        warehouse.ContactNumber = request.ContactNumber?.Trim();
        warehouse.IsActive = request.IsActive;

        await _context.SaveChangesAsync();

        return new WarehouseResponseDto
        {
            WarehouseId = warehouse.WarehouseId,
            WarehouseName = warehouse.WarehouseName,
            Location = warehouse.Location,
            ContactNumber = warehouse.ContactNumber,
            IsActive = warehouse.IsActive
        };
    }

    // DELETE WAREHOUSE
    public async Task<bool> DeleteAsync(int id)
    {
        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.WarehouseId == id);

        if (warehouse == null)
        {
            return false;
        }

        bool hasInventory = await _context.Inventories
            .AnyAsync(i => i.WarehouseId == id);

        bool hasStockTransactions = await _context.StockTransactions
            .AnyAsync(s => s.WarehouseId == id);

        bool hasPurchaseOrders = await _context.PurchaseOrders
            .AnyAsync(p => p.WarehouseId == id);

        bool hasStockTransfers = await _context.StockTransfers
            .AnyAsync(t =>
                t.SourceWarehouseId == id ||
                t.DestinationWarehouseId == id);

        if (hasInventory ||
            hasStockTransactions ||
            hasPurchaseOrders ||
            hasStockTransfers)
        {
            throw new InvalidOperationException(
                "Cannot delete warehouse because it is being used in inventory or operational records.");
        }

        _context.Warehouses.Remove(warehouse);

        await _context.SaveChangesAsync();

        return true;
    }
}
