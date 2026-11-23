
using Microsoft.EntityFrameworkCore;
using SmartInventory.Application.DTOs.Suppliers;
using SmartInventory.Application.Interfaces;
using SmartInventory.Core.Entities;
using SmartInventory.Infrastructure.Data;

namespace SmartInventory.Infrastructure.Services;

public class SupplierService : ISupplierService
{
    private readonly AppDbContext _context;

    public SupplierService(AppDbContext context)
    {
        _context = context;
    }

    // GET ALL SUPPLIERS
    public async Task<List<SupplierResponseDto>> GetAllAsync()
    {
        return await _context.Suppliers
            .AsNoTracking()
            .Select(s => new SupplierResponseDto
            {
                SupplierId = s.SupplierId,
                SupplierName = s.SupplierName,
                Email = s.Email,
                Phone = s.Phone,
                Address = s.Address,
                IsActive = s.IsActive
            })
            .ToListAsync();
    }

    // GET SUPPLIER BY ID
    public async Task<SupplierResponseDto?> GetByIdAsync(int id)
    {
        return await _context.Suppliers
            .AsNoTracking()
            .Where(s => s.SupplierId == id)
            .Select(s => new SupplierResponseDto
            {
                SupplierId = s.SupplierId,
                SupplierName = s.SupplierName,
                Email = s.Email,
                Phone = s.Phone,
                Address = s.Address,
                IsActive = s.IsActive
            })
            .FirstOrDefaultAsync();
    }

    // CREATE SUPPLIER
    public async Task<SupplierResponseDto> CreateAsync(
        CreateSupplierDto request)
    {
        var supplierName = request.SupplierName.Trim();

        var exists = await _context.Suppliers
            .AnyAsync(s => s.SupplierName == supplierName);

        if (exists)
            throw new InvalidOperationException(
                "Supplier name already exists.");

        var supplier = new Supplier
        {
            SupplierName = supplierName,
            Email = request.Email?.Trim(),
            Phone = request.Phone?.Trim(),
            Address = request.Address?.Trim(),
            IsActive = true
        };

        _context.Suppliers.Add(supplier);

        await _context.SaveChangesAsync();

        return new SupplierResponseDto
        {
            SupplierId = supplier.SupplierId,
            SupplierName = supplier.SupplierName,
            Email = supplier.Email,
            Phone = supplier.Phone,
            Address = supplier.Address,
            IsActive = supplier.IsActive
        };
    }

    // UPDATE SUPPLIER
    public async Task<SupplierResponseDto?> UpdateAsync(
        int id,
        UpdateSupplierDto request)
    {
        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.SupplierId == id);

        if (supplier == null)
            return null;

        var supplierName = request.SupplierName.Trim();

        var exists = await _context.Suppliers
            .AnyAsync(s =>
                s.SupplierName == supplierName &&
                s.SupplierId != id);

        if (exists)
            throw new InvalidOperationException(
                "Supplier name already exists.");

        supplier.SupplierName = supplierName;
        supplier.Email = request.Email?.Trim();
        supplier.Phone = request.Phone?.Trim();
        supplier.Address = request.Address?.Trim();
        supplier.IsActive = request.IsActive;

        await _context.SaveChangesAsync();

        return new SupplierResponseDto
        {
            SupplierId = supplier.SupplierId,
            SupplierName = supplier.SupplierName,
            Email = supplier.Email,
            Phone = supplier.Phone,
            Address = supplier.Address,
            IsActive = supplier.IsActive
        };
    }

    // DELETE SUPPLIER
    public async Task<bool> DeleteAsync(int id)
    {
        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.SupplierId == id);

        if (supplier == null)
            return false;

        var hasPurchaseOrders = await _context.PurchaseOrders
            .AnyAsync(p => p.SupplierId == id);

        if (hasPurchaseOrders)
        {
            throw new InvalidOperationException(
                "Cannot delete supplier because purchase orders are linked to it.");
        }

        _context.Suppliers.Remove(supplier);

        await _context.SaveChangesAsync();

        return true;
    }
}
