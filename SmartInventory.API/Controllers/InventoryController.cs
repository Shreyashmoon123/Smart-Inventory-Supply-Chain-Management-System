
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInventory.Application.DTOs.Inventory;
using SmartInventory.Application.Interfaces;

namespace SmartInventory.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    // GET: api/Inventory
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var inventory = await _inventoryService.GetAllAsync();
        return Ok(inventory);
    }

    // GET: api/Inventory/1
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var inventory = await _inventoryService.GetByIdAsync(id);

        if (inventory == null)
        {
            return NotFound(new { message = "Inventory record not found." });
        }

        return Ok(inventory);
    }

    // GET: api/Inventory/product/1
    [HttpGet("product/{productId:int}")]
    public async Task<IActionResult> GetByProduct(int productId)
    {
        var inventory = await _inventoryService.GetByProductAsync(productId);
        return Ok(inventory);
    }

    // GET: api/Inventory/warehouse/1
    [HttpGet("warehouse/{warehouseId:int}")]
    public async Task<IActionResult> GetByWarehouse(int warehouseId)
    {
        var inventory = await _inventoryService.GetByWarehouseAsync(warehouseId);
        return Ok(inventory);
    }

    // GET: api/Inventory/low-stock
    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStock()
    {
        var inventory = await _inventoryService.GetLowStockAsync();
        return Ok(inventory);
    }

    // GET: api/Inventory/transactions
    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] int? productId,
        [FromQuery] int? warehouseId)
    {
        var transactions = await _inventoryService.GetTransactionsAsync(
            productId,
            warehouseId);

        return Ok(transactions);
    }

    // POST: api/Inventory/stock-in
    [HttpPost("stock-in")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> StockIn(StockInDto request)
    {
        try
        {
            int? userId = GetCurrentUserId();

            var inventory = await _inventoryService.StockInAsync(
                request,
                userId);

            return Ok(inventory);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // POST: api/Inventory/stock-out
    [HttpPost("stock-out")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> StockOut(StockOutDto request)
    {
        try
        {
            int? userId = GetCurrentUserId();

            var inventory = await _inventoryService.StockOutAsync(
                request,
                userId);

            return Ok(inventory);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // POST: api/Inventory/adjust
    [HttpPost("adjust")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdjustStock(AdjustStockDto request)
    {
        try
        {
            int? userId = GetCurrentUserId();

            var inventory = await _inventoryService.AdjustStockAsync(
                request,
                userId);

            return Ok(inventory);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    private int? GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(userId, out int parsedUserId)
            ? parsedUserId
            : null;
    }
}
