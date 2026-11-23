
using Microsoft.EntityFrameworkCore;
using SmartInventory.Application.DTOs.PurchaseOrders;
using SmartInventory.Application.Interfaces;
using SmartInventory.Core.Entities;
using SmartInventory.Core.Enums;
using SmartInventory.Infrastructure.Data;

namespace SmartInventory.Infrastructure.Services;

public class PurchaseOrderService : IPurchaseOrderService
{
    private readonly AppDbContext _context;

    public PurchaseOrderService(AppDbContext context)
    {
        _context = context;
    }

    // GET ALL PURCHASE ORDERS
    public async Task<List<PurchaseOrderResponseDto>> GetAllAsync()
    {
        var orders = await _context.PurchaseOrders
            .AsNoTracking()
            .Include(p => p.Supplier)
            .Include(p => p.Warehouse)
            .Include(p => p.Items)
                .ThenInclude(i => i.Product)
            .OrderByDescending(p => p.OrderDate)
            .ToListAsync();

        return orders.Select(MapToResponse).ToList();
    }

    // GET PURCHASE ORDER BY ID
    public async Task<PurchaseOrderResponseDto?> GetByIdAsync(int id)
    {
        var order = await _context.PurchaseOrders
            .AsNoTracking()
            .Include(p => p.Supplier)
            .Include(p => p.Warehouse)
            .Include(p => p.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(p => p.PurchaseOrderId == id);

        return order == null ? null : MapToResponse(order);
    }

    // CREATE PURCHASE ORDER
    public async Task<PurchaseOrderResponseDto> CreateAsync(
        CreatePurchaseOrderDto request)
    {
        ValidateItems(request.Items);

        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s =>
                s.SupplierId == request.SupplierId && s.IsActive);

        if (supplier == null)
            throw new InvalidOperationException(
                "Supplier not found or inactive.");

        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w =>
                w.WarehouseId == request.WarehouseId && w.IsActive);

        if (warehouse == null)
            throw new InvalidOperationException(
                "Warehouse not found or inactive.");

        var productIds = request.Items
            .Select(i => i.ProductId)
            .ToList();

        var products = await _context.Products
            .Where(p => productIds.Contains(p.ProductId) && p.IsActive)
            .ToListAsync();

        if (products.Count != productIds.Distinct().Count())
            throw new InvalidOperationException(
                "One or more products not found or inactive.");

        var order = new PurchaseOrder
        {
            SupplierId = request.SupplierId,
            WarehouseId = request.WarehouseId,
            ExpectedDeliveryDate = request.ExpectedDeliveryDate,
            Notes = request.Notes,
            OrderDate = DateTime.UtcNow,
            Status = PurchaseOrderStatus.Pending
        };

        foreach (var item in request.Items)
        {
            order.Items.Add(new PurchaseOrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                ReceivedQuantity = 0,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.Quantity * item.UnitPrice
            });
        }

        order.TotalAmount = order.Items.Sum(i => i.TotalPrice);

        _context.PurchaseOrders.Add(order);

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(order.PurchaseOrderId))!;
    }

    // UPDATE PENDING PURCHASE ORDER
    public async Task<PurchaseOrderResponseDto> UpdateAsync(
        int id,
        UpdatePurchaseOrderDto request)
    {
        ValidateItems(request.Items);

        var order = await _context.PurchaseOrders
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.PurchaseOrderId == id);

        if (order == null)
            throw new InvalidOperationException(
                "Purchase order not found.");

        if (order.Status != PurchaseOrderStatus.Pending)
            throw new InvalidOperationException(
                "Only pending purchase orders can be updated.");

        var supplier = await _context.Suppliers
            .AnyAsync(s =>
                s.SupplierId == request.SupplierId && s.IsActive);

        if (!supplier)
            throw new InvalidOperationException(
                "Supplier not found or inactive.");

        var warehouse = await _context.Warehouses
            .AnyAsync(w =>
                w.WarehouseId == request.WarehouseId && w.IsActive);

        if (!warehouse)
            throw new InvalidOperationException(
                "Warehouse not found or inactive.");

        var productIds = request.Items
            .Select(i => i.ProductId)
            .ToList();

        var activeProductCount = await _context.Products
            .CountAsync(p =>
                productIds.Contains(p.ProductId) && p.IsActive);

        if (activeProductCount != productIds.Distinct().Count())
            throw new InvalidOperationException(
                "One or more products not found or inactive.");

        order.SupplierId = request.SupplierId;
        order.WarehouseId = request.WarehouseId;
        order.ExpectedDeliveryDate = request.ExpectedDeliveryDate;
        order.Notes = request.Notes;

        _context.PurchaseOrderItems.RemoveRange(order.Items);

        order.Items = request.Items.Select(item =>
            new PurchaseOrderItem
            {
                PurchaseOrderId = id,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                ReceivedQuantity = 0,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.Quantity * item.UnitPrice
            }).ToList();

        order.TotalAmount = order.Items.Sum(i => i.TotalPrice);

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(id))!;
    }

    // APPROVE PURCHASE ORDER
    public async Task<PurchaseOrderResponseDto> ApproveAsync(int id)
    {
        var order = await _context.PurchaseOrders
            .FirstOrDefaultAsync(p => p.PurchaseOrderId == id);

        if (order == null)
            throw new InvalidOperationException(
                "Purchase order not found.");

        if (order.Status != PurchaseOrderStatus.Pending)
            throw new InvalidOperationException(
                "Only pending purchase orders can be approved.");

        order.Status = PurchaseOrderStatus.Approved;

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(id))!;
    }

    // RECEIVE PURCHASE ITEMS
    public async Task<PurchaseOrderResponseDto> ReceiveAsync(
        int id,
        ReceivePurchaseOrderDto request,
        int? userId)
    {
        if (request.Items == null || request.Items.Count == 0)
            throw new InvalidOperationException(
                "At least one receiving item is required.");

        if (request.Items
            .GroupBy(i => i.PurchaseOrderItemId)
            .Any(g => g.Count() > 1))
            throw new InvalidOperationException(
                "Duplicate purchase order item IDs are not allowed.");

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var order = await _context.PurchaseOrders
                .Include(p => p.Items)
                    .ThenInclude(i => i.Product)
                .Include(p => p.Warehouse)
                .FirstOrDefaultAsync(p => p.PurchaseOrderId == id);

            if (order == null)
                throw new InvalidOperationException(
                    "Purchase order not found.");

            if (order.Status != PurchaseOrderStatus.Approved &&
                order.Status != PurchaseOrderStatus.PartiallyReceived)
                throw new InvalidOperationException(
                    "Only approved or partially received orders can be received.");

            if (!order.Warehouse.IsActive)
                throw new InvalidOperationException(
                    "Purchase order warehouse is inactive.");

            foreach (var receivedItem in request.Items)
            {
                var orderItem = order.Items.FirstOrDefault(i =>
                    i.PurchaseOrderItemId ==
                    receivedItem.PurchaseOrderItemId);

                if (orderItem == null)
                    throw new InvalidOperationException(
                        $"Purchase order item {receivedItem.PurchaseOrderItemId} not found.");

                if (receivedItem.ReceivedQuantity <= 0)
                    throw new InvalidOperationException(
                        "Received quantity must be greater than zero.");

                var remainingQuantity =
                    orderItem.Quantity - orderItem.ReceivedQuantity;

                if (receivedItem.ReceivedQuantity > remainingQuantity)
                    throw new InvalidOperationException(
                        $"Received quantity exceeds remaining quantity for product {orderItem.ProductId}.");

                if (!orderItem.Product.IsActive)
                    throw new InvalidOperationException(
                        $"Product {orderItem.ProductId} is inactive.");

                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i =>
                        i.ProductId == orderItem.ProductId &&
                        i.WarehouseId == order.WarehouseId);

                if (inventory == null)
                {
                    inventory = new Inventory
                    {
                        ProductId = orderItem.ProductId,
                        WarehouseId = order.WarehouseId,
                        Quantity = 0,
                        LastUpdated = DateTime.UtcNow
                    };

                    _context.Inventories.Add(inventory);
                }

                inventory.Quantity += receivedItem.ReceivedQuantity;
                inventory.LastUpdated = DateTime.UtcNow;

                orderItem.ReceivedQuantity +=
                    receivedItem.ReceivedQuantity;

                _context.StockTransactions.Add(new StockTransaction
                {
                    ProductId = orderItem.ProductId,
                    WarehouseId = order.WarehouseId,
                    UserId = userId,
                    TransactionType = StockTransactionType.StockIn,
                    Quantity = receivedItem.ReceivedQuantity,
                    ReferenceType = "PurchaseOrder",
                    ReferenceId = order.PurchaseOrderId,
                    Notes = request.Notes ?? "Stock received against purchase order",
                    TransactionDate = DateTime.UtcNow
                });
            }

            bool allReceived = order.Items.All(i =>
                i.ReceivedQuantity >= i.Quantity);

            order.Status = allReceived
                ? PurchaseOrderStatus.Received
                : PurchaseOrderStatus.PartiallyReceived;

            if (allReceived)
                order.ReceivedDate = DateTime.UtcNow;

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

    // CANCEL PURCHASE ORDER
    public async Task<PurchaseOrderResponseDto> CancelAsync(int id)
    {
        var order = await _context.PurchaseOrders
            .FirstOrDefaultAsync(p => p.PurchaseOrderId == id);

        if (order == null)
            throw new InvalidOperationException(
                "Purchase order not found.");

        if (order.Status != PurchaseOrderStatus.Pending &&
            order.Status != PurchaseOrderStatus.Approved)
            throw new InvalidOperationException(
                "Only pending or approved orders can be cancelled.");

        order.Status = PurchaseOrderStatus.Cancelled;

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(id))!;
    }

    // VALIDATE ITEMS
    private static void ValidateItems(
        List<CreatePurchaseOrderItemDto> items)
    {
        if (items == null || items.Count == 0)
            throw new InvalidOperationException(
                "At least one product is required.");

        if (items.GroupBy(i => i.ProductId).Any(g => g.Count() > 1))
            throw new InvalidOperationException(
                "Duplicate products are not allowed in one purchase order.");

        if (items.Any(i => i.Quantity <= 0 || i.UnitPrice <= 0))
            throw new InvalidOperationException(
                "Quantity and unit price must be greater than zero.");
    }

    // RESPONSE MAPPING
    private static PurchaseOrderResponseDto MapToResponse(
        PurchaseOrder order)
    {
        return new PurchaseOrderResponseDto
        {
            PurchaseOrderId = order.PurchaseOrderId,

            SupplierId = order.SupplierId,
            SupplierName = order.Supplier.SupplierName,

            WarehouseId = order.WarehouseId,
            WarehouseName = order.Warehouse.WarehouseName,

            OrderDate = order.OrderDate,
            ExpectedDeliveryDate = order.ExpectedDeliveryDate,
            ReceivedDate = order.ReceivedDate,

            Status = order.Status,
            TotalAmount = order.TotalAmount,
            Notes = order.Notes,

            Items = order.Items.Select(i =>
                new PurchaseOrderItemResponseDto
                {
                    PurchaseOrderItemId = i.PurchaseOrderItemId,
                    ProductId = i.ProductId,
                    ProductName = i.Product.ProductName,
                    SKU = i.Product.SKU,
                    Quantity = i.Quantity,
                    ReceivedQuantity = i.ReceivedQuantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice
                }).ToList()
        };
    }
}
