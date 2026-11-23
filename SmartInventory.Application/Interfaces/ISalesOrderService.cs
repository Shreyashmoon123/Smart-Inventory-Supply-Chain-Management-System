
using SmartInventory.Application.DTOs.SalesOrders;

namespace SmartInventory.Application.Interfaces;

public interface ISalesOrderService
{
    Task<List<SalesOrderResponseDto>> GetAllAsync();
    Task<SalesOrderResponseDto?> GetByIdAsync(int id);
    Task<SalesOrderResponseDto> CreateAsync(CreateSalesOrderDto dto);
    Task<SalesOrderResponseDto> ConfirmAsync(int id, int? userId);
    Task<SalesOrderResponseDto> UpdateStatusAsync(int id, string status);
    Task CancelAsync(int id);
}
