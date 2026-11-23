
using SmartInventory.Application.DTOs.Payments;

namespace SmartInventory.Application.Interfaces;

public interface IPaymentService
{
    Task<List<PaymentResponseDto>> GetAllAsync();
    Task<PaymentResponseDto?> GetByIdAsync(int id);
    Task<List<PaymentResponseDto>> GetBySalesOrderIdAsync(int salesOrderId);
    Task<PaymentSummaryDto> GetSummaryAsync(int salesOrderId);
    Task<PaymentResponseDto> CreateAsync(CreatePaymentDto dto);
    Task<PaymentResponseDto> UpdateStatusAsync(int id, string status);
}
