
using Microsoft.EntityFrameworkCore;
using SmartInventory.Application.DTOs.StockTransfers;
using SmartInventory.Application.Interfaces;
using SmartInventory.Core.Entities;
using SmartInventory.Core.Enums;
using SmartInventory.Infrastructure.Data;

namespace SmartInventory.Infrastructure.Services;

public class StockTransferService : IStockTransferService
{
    private readonly AppDbContext _context;

    public StockTransferService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<StockTransferResponseDto>> GetAllAsync()
    {
        var transfers = await _context.StockTransfers
            .Include(x => x.SourceWarehouse)
            .Include(x => x.DestinationWarehouse)
            .Include(x => x.Items)
                .ThenInclude(x => x.Product)
            .OrderByDescending(x => x.TransferDate)
            .ToListAsync();

        return transfers.Select(MapToResponse).ToList();
    }

    public async Task<StockTransferResponseDto?> GetByIdAsync(int id)
    {
        var transfer = await _context.StockTransfers
            .Include(x => x.SourceWarehouse)
            .Include(x => x.DestinationWarehouse)
            .Include(x => x.Items)
                .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.StockTransferId == id);

        return transfer == null ? null : MapToResponse(transfer);
    }

    public async Task<StockTransferResponseDto> CreateAsync(
        CreateStockTransferDto dto)
    {
        if (dto.SourceWarehouseId == dto.DestinationWarehouseId)
            throw new InvalidOperationException(
                "Source and destination warehouses must be different.");

        if (dto.Items == null || dto.Items.Count == 0)
            throw new InvalidOperationException(
                "At least one product is required.");

        if (dto.Items.GroupBy(x => x.ProductId).Any(g => g.Count() > 1))
            throw new InvalidOperationException(
                "Duplicate products are not allowed.");

        var source = await _context.Warehouses
            .FirstOrDefaultAsync(x =>
                x.WarehouseId == dto.SourceWarehouseId && x.IsActive);

        if (source == null)
            throw new InvalidOperationException(
                "Active source warehouse not found.");

        var destination = await _context.Warehouses
            .FirstOrDefaultAsync(x =>
                x.WarehouseId == dto.DestinationWarehouseId && x.IsActive);

        if (destination == null)
            throw new InvalidOperationException(
                "Active destination warehouse not found.");

        var transfer = new StockTransfer
        {
            SourceWarehouseId = dto.SourceWarehouseId,
            DestinationWarehouseId = dto.DestinationWarehouseId,
            TransferDate = DateTime.UtcNow,
            Status = StockTransferStatus.Pending,
            Notes = dto.Notes
        };

        foreach (var item in dto.Items)
        {
            if (item.Quantity <= 0)
                throw new InvalidOperationException(
                    "Transfer quantity must be greater than zero.");

            var product = await _context.Products
                .FirstOrDefaultAsync(x =>
                    x.ProductId == item.ProductId && x.IsActive);

            if (product == null)
                throw new InvalidOperationException(
                    $"Active product with ID {item.ProductId} not found.");

            transfer.Items.Add(new StockTransferItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity
            });
        }

        _context.StockTransfers.Add(transfer);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(transfer.StockTransferId))!;
    }

    public async Task<StockTransferResponseDto> ApproveAsync(int id)
    {
        var transfer = await _context.StockTransfers
            .FirstOrDefaultAsync(x => x.StockTransferId == id);

        if (transfer == null)
            throw new InvalidOperationException("Transfer not found.");

        if (transfer.Status != StockTransferStatus.Pending)
            throw new InvalidOperationException(
                "Only pending transfers can be approved.");

        transfer.Status = StockTransferStatus.Approved;

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(id))!;
    }

    public async Task<StockTransferResponseDto> DispatchAsync(
        int id, int? userId)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var transfer = await _context.StockTransfers
                .Include(x => x.Items)
                    .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x => x.StockTransferId == id);

            if (transfer == null)
                throw new InvalidOperationException("Transfer not found.");

            if (transfer.Status != StockTransferStatus.Approved)
                throw new InvalidOperationException(
                    "Only approved transfers can be dispatched.");

            var source = await _context.Warehouses
                .FirstOrDefaultAsync(x =>
                    x.WarehouseId == transfer.SourceWarehouseId &&
                    x.IsActive);

            if (source == null)
                throw new InvalidOperationException(
                    "Source warehouse is inactive or not found.");

            foreach (var item in transfer.Items)
            {
                if (!item.Product.IsActive)
                    throw new InvalidOperationException(
                        $"Product {item.Product.ProductName} is inactive.");

                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(x =>
                        x.ProductId == item.ProductId &&
                        x.WarehouseId == transfer.SourceWarehouseId);

                if (inventory == null || inventory.Quantity < item.Quantity)
                    throw new InvalidOperationException(
                        $"Insufficient source stock for {item.Product.ProductName}.");

                inventory.Quantity -= item.Quantity;
                inventory.LastUpdated = DateTime.UtcNow;

                _context.StockTransactions.Add(new StockTransaction
                {
                    ProductId = item.ProductId,
                    WarehouseId = transfer.SourceWarehouseId,
                    UserId = userId,
                    TransactionType = StockTransactionType.TransferOut,
                    Quantity = item.Quantity,
                    ReferenceType = "StockTransfer",
                    ReferenceId = transfer.StockTransferId,
                    Notes = $"Stock dispatched for transfer #{transfer.StockTransferId}",
                    TransactionDate = DateTime.UtcNow
                });
            }

            transfer.Status = StockTransferStatus.InTransit;

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

    public async Task<StockTransferResponseDto> CompleteAsync(
        int id, int? userId)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var transfer = await _context.StockTransfers
                .Include(x => x.Items)
                    .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x => x.StockTransferId == id);

            if (transfer == null)
                throw new InvalidOperationException("Transfer not found.");

            if (transfer.Status != StockTransferStatus.InTransit)
                throw new InvalidOperationException(
                    "Only in-transit transfers can be completed.");

            var destination = await _context.Warehouses
                .FirstOrDefaultAsync(x =>
                    x.WarehouseId == transfer.DestinationWarehouseId &&
                    x.IsActive);

            if (destination == null)
                throw new InvalidOperationException(
                    "Destination warehouse is inactive or not found.");

            foreach (var item in transfer.Items)
            {
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(x =>
                        x.ProductId == item.ProductId &&
                        x.WarehouseId == transfer.DestinationWarehouseId);

                if (inventory == null)
                {
                    inventory = new Inventory
                    {
                        ProductId = item.ProductId,
                        WarehouseId = transfer.DestinationWarehouseId,
                        Quantity = 0,
                        LastUpdated = DateTime.UtcNow
                    };

                    _context.Inventories.Add(inventory);
                }

                inventory.Quantity += item.Quantity;
                inventory.LastUpdated = DateTime.UtcNow;

                _context.StockTransactions.Add(new StockTransaction
                {
                    ProductId = item.ProductId,
                    WarehouseId = transfer.DestinationWarehouseId,
                    UserId = userId,
                    TransactionType = StockTransactionType.TransferIn,
                    Quantity = item.Quantity,
                    ReferenceType = "StockTransfer",
                    ReferenceId = transfer.StockTransferId,
                    Notes = $"Stock received for transfer #{transfer.StockTransferId}",
                    TransactionDate = DateTime.UtcNow
                });
            }

            transfer.Status = StockTransferStatus.Completed;

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

    public async Task CancelAsync(int id)
    {
        var transfer = await _context.StockTransfers
            .FirstOrDefaultAsync(x => x.StockTransferId == id);

        if (transfer == null)
            throw new InvalidOperationException("Transfer not found.");

        if (transfer.Status != StockTransferStatus.Pending &&
            transfer.Status != StockTransferStatus.Approved)
        {
            throw new InvalidOperationException(
                "Only pending or approved transfers can be cancelled.");
        }

        transfer.Status = StockTransferStatus.Cancelled;

        await _context.SaveChangesAsync();
    }

    private static StockTransferResponseDto MapToResponse(
        StockTransfer transfer)
    {
        return new StockTransferResponseDto
        {
            StockTransferId = transfer.StockTransferId,
            SourceWarehouseId = transfer.SourceWarehouseId,
            SourceWarehouseName = transfer.SourceWarehouse.WarehouseName,
            DestinationWarehouseId = transfer.DestinationWarehouseId,
            DestinationWarehouseName = transfer.DestinationWarehouse.WarehouseName,
            TransferDate = transfer.TransferDate,
            Status = transfer.Status.ToString(),
            Notes = transfer.Notes,
            Items = transfer.Items.Select(item =>
                new StockTransferItemResponseDto
                {
                    StockTransferItemId = item.StockTransferItemId,
                    ProductId = item.ProductId,
                    ProductName = item.Product.ProductName,
                    SKU = item.Product.SKU,
                    Quantity = item.Quantity
                }).ToList()
        };
    }
}
