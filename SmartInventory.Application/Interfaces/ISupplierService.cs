
using SmartInventory.Application.DTOs.Suppliers;

namespace SmartInventory.Application.Interfaces;

public interface ISupplierService
{
    Task<List<SupplierResponseDto>> GetAllAsync();

    Task<SupplierResponseDto?> GetByIdAsync(int id);

    Task<SupplierResponseDto> CreateAsync(CreateSupplierDto request);

    Task<SupplierResponseDto?> UpdateAsync(
        int id,
        UpdateSupplierDto request);

    Task<bool> DeleteAsync(int id);
}
