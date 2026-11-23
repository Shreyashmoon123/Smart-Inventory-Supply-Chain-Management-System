using Microsoft.EntityFrameworkCore;
using SmartInventory.Application.DTOs.Categories;
using SmartInventory.Application.Interfaces;
using SmartInventory.Core.Entities;
using SmartInventory.Infrastructure.Data;

namespace SmartInventory.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;

    public CategoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CategoryResponseDto>> GetAllAsync()
    {
        return await _context.Categories
            .AsNoTracking()
            .Select(c => new CategoryResponseDto
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                Description = c.Description,
                IsActive = c.IsActive
            })
            .ToListAsync();
    }

    public async Task<CategoryResponseDto?> GetByIdAsync(int id)
    {
        return await _context.Categories
            .AsNoTracking()
            .Where(c => c.CategoryId == id)
            .Select(c => new CategoryResponseDto
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                Description = c.Description,
                IsActive = c.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<CategoryResponseDto> CreateAsync(CreateCategoryDto request)
    {
        var name = request.CategoryName.Trim();

        var exists = await _context.Categories
            .AnyAsync(c => c.CategoryName == name);

        if (exists)
            throw new InvalidOperationException("Category already exists.");

        var category = new Category
        {
            CategoryName = name,
            Description = request.Description?.Trim(),
            IsActive = true
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return new CategoryResponseDto
        {
            CategoryId = category.CategoryId,
            CategoryName = category.CategoryName,
            Description = category.Description,
            IsActive = category.IsActive
        };
    }

    public async Task<CategoryResponseDto?> UpdateAsync(
        int id,
        UpdateCategoryDto request)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
            return null;

        var name = request.CategoryName.Trim();

        var exists = await _context.Categories
            .AnyAsync(c => c.CategoryName == name && c.CategoryId != id);

        if (exists)
            throw new InvalidOperationException("Category name already exists.");

        category.CategoryName = name;
        category.Description = request.Description?.Trim();
        category.IsActive = request.IsActive;

        await _context.SaveChangesAsync();

        return new CategoryResponseDto
        {
            CategoryId = category.CategoryId,
            CategoryName = category.CategoryName,
            Description = category.Description,
            IsActive = category.IsActive
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
            return false;

        var hasProducts = await _context.Products
            .AnyAsync(p => p.CategoryId == id);

        if (hasProducts)
            throw new InvalidOperationException(
                "Cannot delete category because products are assigned to it.");

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return true;
    }
}