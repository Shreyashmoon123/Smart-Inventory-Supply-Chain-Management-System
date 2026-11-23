
using SmartInventory.Application.DTOs.Inventory;

namespace SmartInventory.Application.Interfaces;

public interface IInventoryService
{
    Task<List<InventoryResponseDto>> GetAllAsync();

    Task<InventoryResponseDto?> GetByIdAsync(int id);

    Task<List<InventoryResponseDto>> GetByProductAsync(int productId);

    Task<List<InventoryResponseDto>> GetByWarehouseAsync(int warehouseId);

    Task<List<InventoryResponseDto>> GetLowStockAsync();

    Task<List<StockTransactionResponseDto>> GetTransactionsAsync(
        int? productId,
        int? warehouseId);

    Task<InventoryResponseDto> StockInAsync(
        StockInDto request,
        int? userId);

    Task<InventoryResponseDto> StockOutAsync(
        StockOutDto request,
        int? userId);

    Task<InventoryResponseDto> AdjustStockAsync(
        AdjustStockDto request,
        int? userId);
}
