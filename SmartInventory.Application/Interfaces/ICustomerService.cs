
using SmartInventory.Application.DTOs.Customers;

namespace SmartInventory.Application.Interfaces;

public interface ICustomerService
{
    Task<List<CustomerResponseDto>> GetAllAsync();
    Task<CustomerResponseDto?> GetByIdAsync(int id);
    Task<CustomerResponseDto> CreateAsync(CreateCustomerDto dto);
    Task<CustomerResponseDto> UpdateAsync(int id, UpdateCustomerDto dto);
    Task DeleteAsync(int id);
}
