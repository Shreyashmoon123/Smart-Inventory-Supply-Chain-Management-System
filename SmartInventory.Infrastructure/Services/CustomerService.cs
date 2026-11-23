
using Microsoft.EntityFrameworkCore;
using SmartInventory.Application.DTOs.Customers;
using SmartInventory.Application.Interfaces;
using SmartInventory.Core.Entities;
using SmartInventory.Infrastructure.Data;

namespace SmartInventory.Infrastructure.Services;

public class CustomerService : ICustomerService
{
    private readonly AppDbContext _context;

    public CustomerService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CustomerResponseDto>> GetAllAsync()
    {
        var customers = await _context.Customers
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return customers.Select(MapToResponse).ToList();
    }

    public async Task<CustomerResponseDto?> GetByIdAsync(int id)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(x => x.CustomerId == id);

        return customer == null ? null : MapToResponse(customer);
    }

    public async Task<CustomerResponseDto> CreateAsync(CreateCustomerDto dto)
    {
        var email = NormalizeEmail(dto.Email);

        if (email != null && await _context.Customers.AnyAsync(
            x => x.Email != null && x.Email.ToLower() == email))
        {
            throw new InvalidOperationException(
                "A customer with this email already exists.");
        }

        var customer = new Customer
        {
            CustomerName = dto.CustomerName.Trim(),
            Email = email,
            Phone = dto.Phone?.Trim(),
            Address = dto.Address?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return MapToResponse(customer);
    }

    public async Task<CustomerResponseDto> UpdateAsync(
        int id, UpdateCustomerDto dto)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(x => x.CustomerId == id);

        if (customer == null)
            throw new InvalidOperationException("Customer not found.");

        var email = NormalizeEmail(dto.Email);

        if (email != null && await _context.Customers.AnyAsync(
            x => x.CustomerId != id &&
                 x.Email != null &&
                 x.Email.ToLower() == email))
        {
            throw new InvalidOperationException(
                "Another customer already uses this email.");
        }

        customer.CustomerName = dto.CustomerName.Trim();
        customer.Email = email;
        customer.Phone = dto.Phone?.Trim();
        customer.Address = dto.Address?.Trim();

        await _context.SaveChangesAsync();

        return MapToResponse(customer);
    }

    public async Task DeleteAsync(int id)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(x => x.CustomerId == id);

        if (customer == null)
            throw new InvalidOperationException("Customer not found.");

        var hasOrders = await _context.SalesOrders
            .AnyAsync(x => x.CustomerId == id);

        if (hasOrders)
            throw new InvalidOperationException(
                "Customer cannot be deleted because sales orders are linked to this customer.");

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();
    }

    private static string? NormalizeEmail(string? email)
    {
        return string.IsNullOrWhiteSpace(email)
            ? null
            : email.Trim().ToLowerInvariant();
    }

    private static CustomerResponseDto MapToResponse(Customer customer)
    {
        return new CustomerResponseDto
        {
            CustomerId = customer.CustomerId,
            CustomerName = customer.CustomerName,
            Email = customer.Email,
            Phone = customer.Phone,
            Address = customer.Address,
            CreatedAt = customer.CreatedAt
        };
    }
}
