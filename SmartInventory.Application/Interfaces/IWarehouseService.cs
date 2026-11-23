
using SmartInventory.Application.DTOs.Warehouses;

namespace SmartInventory.Application.Interfaces;

public interface IWarehouseService
{
    Task<List<WarehouseResponseDto>> GetAllAsync();

    Task<WarehouseResponseDto?> GetByIdAsync(int id);

    Task<WarehouseResponseDto> CreateAsync(CreateWarehouseDto request);

    Task<WarehouseResponseDto?> UpdateAsync(
        int id,
        UpdateWarehouseDto request);

    Task<bool> DeleteAsync(int id);
}
