
using Microsoft.EntityFrameworkCore;
using SmartInventory.Application.DTOs.Products;
using SmartInventory.Application.Interfaces;
using SmartInventory.Core.Entities;
using SmartInventory.Infrastructure.Data;

namespace SmartInventory.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;

    public ProductService(AppDbContext context)
    {
        _context = context;
    }

    // GET ALL PRODUCTS
    public async Task<List<ProductResponseDto>> GetAllAsync()
    {
        return await _context.Products
            .AsNoTracking()
            .Select(p => new ProductResponseDto
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                SKU = p.SKU,
                Description = p.Description,
                PurchasePrice = p.PurchasePrice,
                SellingPrice = p.SellingPrice,
                ReorderLevel = p.ReorderLevel,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.CategoryName,
                IsActive = p.IsActive
            })
            .ToListAsync();
    }

    // GET PRODUCT BY ID
    public async Task<ProductResponseDto?> GetByIdAsync(int id)
    {
        return await _context.Products
            .AsNoTracking()
            .Where(p => p.ProductId == id)
            .Select(p => new ProductResponseDto
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                SKU = p.SKU,
                Description = p.Description,
                PurchasePrice = p.PurchasePrice,
                SellingPrice = p.SellingPrice,
                ReorderLevel = p.ReorderLevel,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.CategoryName,
                IsActive = p.IsActive
            })
            .FirstOrDefaultAsync();
    }

    // CREATE PRODUCT
    public async Task<ProductResponseDto> CreateAsync(
        CreateProductDto request)
    {
        var sku = request.SKU.Trim();

        var skuExists = await _context.Products
            .AnyAsync(p => p.SKU == sku);

        if (skuExists)
            throw new InvalidOperationException("SKU already exists.");

        var category = await _context.Categories
            .FirstOrDefaultAsync(c =>
                c.CategoryId == request.CategoryId && c.IsActive);

        if (category == null)
            throw new InvalidOperationException(
                "Category not found or inactive.");

        var product = new Product
        {
            ProductName = request.ProductName.Trim(),
            SKU = sku,
            Description = request.Description?.Trim(),
            PurchasePrice = request.PurchasePrice,
            SellingPrice = request.SellingPrice,
            ReorderLevel = request.ReorderLevel,
            CategoryId = request.CategoryId,
            IsActive = true
        };

        _context.Products.Add(product);

        await _context.SaveChangesAsync();

        return new ProductResponseDto
        {
            ProductId = product.ProductId,
            ProductName = product.ProductName,
            SKU = product.SKU,
            Description = product.Description,
            PurchasePrice = product.PurchasePrice,
            SellingPrice = product.SellingPrice,
            ReorderLevel = product.ReorderLevel,
            CategoryId = product.CategoryId,
            CategoryName = category.CategoryName,
            IsActive = product.IsActive
        };
    }

    // UPDATE PRODUCT
    public async Task<ProductResponseDto?> UpdateAsync(
        int id,
        UpdateProductDto request)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.ProductId == id);

        if (product == null)
            return null;

        var sku = request.SKU.Trim();

        var skuExists = await _context.Products
            .AnyAsync(p => p.SKU == sku && p.ProductId != id);

        if (skuExists)
            throw new InvalidOperationException("SKU already exists.");

        var category = await _context.Categories
            .FirstOrDefaultAsync(c =>
                c.CategoryId == request.CategoryId && c.IsActive);

        if (category == null)
            throw new InvalidOperationException(
                "Category not found or inactive.");

        product.ProductName = request.ProductName.Trim();
        product.SKU = sku;
        product.Description = request.Description?.Trim();
        product.PurchasePrice = request.PurchasePrice;
        product.SellingPrice = request.SellingPrice;
        product.ReorderLevel = request.ReorderLevel;
        product.CategoryId = request.CategoryId;
        product.IsActive = request.IsActive;

        await _context.SaveChangesAsync();

        return new ProductResponseDto
        {
            ProductId = product.ProductId,
            ProductName = product.ProductName,
            SKU = product.SKU,
            Description = product.Description,
            PurchasePrice = product.PurchasePrice,
            SellingPrice = product.SellingPrice,
            ReorderLevel = product.ReorderLevel,
            CategoryId = product.CategoryId,
            CategoryName = category.CategoryName,
            IsActive = product.IsActive
        };
    }

    // DELETE PRODUCT
    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.ProductId == id);

        if (product == null)
            return false;

        var hasInventory = await _context.Inventories
            .AnyAsync(i => i.ProductId == id);

        var hasTransactions = await _context.StockTransactions
            .AnyAsync(t => t.ProductId == id);

        var hasPurchaseItems = await _context.PurchaseOrderItems
            .AnyAsync(i => i.ProductId == id);

        var hasSalesItems = await _context.SalesOrderItems
            .AnyAsync(i => i.ProductId == id);

        var hasTransferItems = await _context.StockTransferItems
            .AnyAsync(i => i.ProductId == id);

        if (hasInventory || hasTransactions ||
            hasPurchaseItems || hasSalesItems || hasTransferItems)
        {
            throw new InvalidOperationException(
                "Cannot delete product because it has existing inventory or transaction records.");
        }

        _context.Products.Remove(product);

        await _context.SaveChangesAsync();

        return true;
    }
}
