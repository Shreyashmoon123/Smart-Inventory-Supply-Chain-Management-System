
using SmartInventory.Application.DTOs.StockTransfers;

namespace SmartInventory.Application.Interfaces;

public interface IStockTransferService
{
    Task<List<StockTransferResponseDto>> GetAllAsync();
    Task<StockTransferResponseDto?> GetByIdAsync(int id);
    Task<StockTransferResponseDto> CreateAsync(CreateStockTransferDto dto);
    Task<StockTransferResponseDto> ApproveAsync(int id);
    Task<StockTransferResponseDto> DispatchAsync(int id, int? userId);
    Task<StockTransferResponseDto> CompleteAsync(int id, int? userId);
    Task CancelAsync(int id);
}
