
using Microsoft.EntityFrameworkCore;
using SmartInventory.Application.DTOs.SalesOrders;
using SmartInventory.Application.Interfaces;
using SmartInventory.Core.Entities;
using SmartInventory.Core.Enums;
using SmartInventory.Infrastructure.Data;

namespace SmartInventory.Infrastructure.Services;

public class SalesOrderService : ISalesOrderService
{
    private readonly AppDbContext _context;

    public SalesOrderService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<SalesOrderResponseDto>> GetAllAsync()
    {
        var orders = await _context.SalesOrders
            .Include(x => x.Customer)
            .Include(x => x.Items)
                .ThenInclude(x => x.Product)
            .OrderByDescending(x => x.OrderDate)
            .ToListAsync();

        return orders.Select(MapToResponse).ToList();
    }

    public async Task<SalesOrderResponseDto?> GetByIdAsync(int id)
    {
        var order = await _context.SalesOrders
            .Include(x => x.Customer)
            .Include(x => x.Items)
                .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.SalesOrderId == id);

        return order == null ? null : MapToResponse(order);
    }

    public async Task<SalesOrderResponseDto> CreateAsync(
        CreateSalesOrderDto dto)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(x => x.CustomerId == dto.CustomerId);

        if (customer == null)
            throw new InvalidOperationException("Customer not found.");

        if (dto.Items == null || dto.Items.Count == 0)
            throw new InvalidOperationException("At least one product is required.");

        if (dto.Items.GroupBy(x => x.ProductId).Any(g => g.Count() > 1))
            throw new InvalidOperationException("Duplicate products are not allowed.");

        var order = new SalesOrder
        {
            CustomerId = dto.CustomerId,
            OrderDate = DateTime.UtcNow,
            Status = SalesOrderStatus.Pending,
            Notes = dto.Notes
        };

        foreach (var item in dto.Items)
        {
            if (item.Quantity <= 0)
                throw new InvalidOperationException("Quantity must be greater than zero.");

            var product = await _context.Products
                .FirstOrDefaultAsync(x =>
                    x.ProductId == item.ProductId && x.IsActive);

            if (product == null)
                throw new InvalidOperationException(
                    $"Active product with ID {item.ProductId} not found.");

            var totalPrice = item.Quantity * product.SellingPrice;

            order.Items.Add(new SalesOrderItem
            {
                ProductId = product.ProductId,
                Quantity = item.Quantity,
                UnitPrice = product.SellingPrice,
                TotalPrice = totalPrice
            });

            order.TotalAmount += totalPrice;
        }

        _context.SalesOrders.Add(order);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(order.SalesOrderId))!;
    }

    public async Task<SalesOrderResponseDto> ConfirmAsync(
        int id, int? userId)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var order = await _context.SalesOrders
                .Include(x => x.Items)
                    .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x => x.SalesOrderId == id);

            if (order == null)
                throw new InvalidOperationException("Sales order not found.");

            if (order.Status != SalesOrderStatus.Pending)
                throw new InvalidOperationException(
                    "Only pending orders can be confirmed.");

            foreach (var item in order.Items)
            {
                if (!item.Product.IsActive)
                    throw new InvalidOperationException(
                        $"Product {item.Product.ProductName} is inactive.");

                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(x =>
                        x.ProductId == item.ProductId &&
                        x.WarehouseId == 1);

                if (inventory == null || inventory.Quantity < item.Quantity)
                    throw new InvalidOperationException(
                        $"Insufficient stock for {item.Product.ProductName}.");

                inventory.Quantity -= item.Quantity;
                inventory.LastUpdated = DateTime.UtcNow;

                _context.StockTransactions.Add(new StockTransaction
                {
                    ProductId = item.ProductId,
                    WarehouseId = inventory.WarehouseId,
                    UserId = userId,
                    TransactionType = StockTransactionType.StockOut,
                    Quantity = item.Quantity,
                    ReferenceType = "SalesOrder",
                    ReferenceId = order.SalesOrderId,
                    Notes = $"Stock deducted for Sales Order #{order.SalesOrderId}",
                    TransactionDate = DateTime.UtcNow
                });
            }

            order.Status = SalesOrderStatus.Confirmed;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (await GetByIdAsync(id))!;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<SalesOrderResponseDto> UpdateStatusAsync(
        int id, string status)
    {
        var order = await _context.SalesOrders
            .Include(x => x.Customer)
            .Include(x => x.Items)
                .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.SalesOrderId == id);

        if (order == null)
            throw new InvalidOperationException("Sales order not found.");

        if (!Enum.TryParse<SalesOrderStatus>(
            status, true, out var newStatus))
        {
            throw new InvalidOperationException("Invalid sales order status.");
        }

        var isValidTransition =
            (order.Status == SalesOrderStatus.Confirmed &&
             newStatus == SalesOrderStatus.Processing) ||
            (order.Status == SalesOrderStatus.Processing &&
             newStatus == SalesOrderStatus.Shipped) ||
            (order.Status == SalesOrderStatus.Shipped &&
             newStatus == SalesOrderStatus.Delivered);

        if (!isValidTransition)
            throw new InvalidOperationException(
                $"Cannot change status from {order.Status} to {newStatus}.");

        order.Status = newStatus;

        await _context.SaveChangesAsync();

        return MapToResponse(order);
    }

    public async Task CancelAsync(int id)
    {
        var order = await _context.SalesOrders
            .FirstOrDefaultAsync(x => x.SalesOrderId == id);

        if (order == null)
            throw new InvalidOperationException("Sales order not found.");

        if (order.Status != SalesOrderStatus.Pending)
            throw new InvalidOperationException(
                "Only pending sales orders can be cancelled.");

        order.Status = SalesOrderStatus.Cancelled;

        await _context.SaveChangesAsync();
    }

    private static SalesOrderResponseDto MapToResponse(
        SalesOrder order)
    {
        return new SalesOrderResponseDto
        {
            SalesOrderId = order.SalesOrderId,
            CustomerId = order.CustomerId,
            CustomerName = order.Customer.CustomerName,
            OrderDate = order.OrderDate,
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            Notes = order.Notes,

            Items = order.Items.Select(item =>
                new SalesOrderItemResponseDto
                {
                    SalesOrderItemId = item.SalesOrderItemId,
                    ProductId = item.ProductId,
                    ProductName = item.Product.ProductName,
                    SKU = item.Product.SKU,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice
                }).ToList()
        };
    }
}
