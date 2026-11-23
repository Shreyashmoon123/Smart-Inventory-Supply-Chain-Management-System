using SmartInventory.Application.DTOs.Products;

namespace SmartInventory.Application.Interfaces;

public interface IProductService
{
    Task<List<ProductResponseDto>> GetAllAsync();

    Task<ProductResponseDto?> GetByIdAsync(int id);

    Task<ProductResponseDto> CreateAsync(CreateProductDto request);

    Task<ProductResponseDto?> UpdateAsync(int id, UpdateProductDto request);

    Task<bool> DeleteAsync(int id);
}