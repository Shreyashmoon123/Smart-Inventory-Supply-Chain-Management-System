
using SmartInventory.Application.DTOs.PurchaseOrders;

namespace SmartInventory.Application.Interfaces;

public interface IPurchaseOrderService
{
    Task<List<PurchaseOrderResponseDto>> GetAllAsync();

    Task<PurchaseOrderResponseDto?> GetByIdAsync(int id);

    Task<PurchaseOrderResponseDto> CreateAsync(
        CreatePurchaseOrderDto request);

    Task<PurchaseOrderResponseDto> UpdateAsync(
        int id,
        UpdatePurchaseOrderDto request);

    Task<PurchaseOrderResponseDto> ApproveAsync(int id);

    Task<PurchaseOrderResponseDto> ReceiveAsync(
        int id,
        ReceivePurchaseOrderDto request,
        int? userId);

    Task<PurchaseOrderResponseDto> CancelAsync(int id);
}
