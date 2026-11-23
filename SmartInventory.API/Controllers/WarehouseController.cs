
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInventory.Application.DTOs.Warehouses;
using SmartInventory.Application.Interfaces;

namespace SmartInventory.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WarehouseController : ControllerBase
{
    private readonly IWarehouseService _warehouseService;

    public WarehouseController(IWarehouseService warehouseService)
    {
        _warehouseService = warehouseService;
    }

    // GET: api/Warehouse
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var warehouses = await _warehouseService.GetAllAsync();

        return Ok(warehouses);
    }

    // GET: api/Warehouse/1
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var warehouse = await _warehouseService.GetByIdAsync(id);

        if (warehouse == null)
        {
            return NotFound(new
            {
                message = "Warehouse not found."
            });
        }

        return Ok(warehouse);
    }

    // POST: api/Warehouse
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create(
        CreateWarehouseDto request)
    {
        try
        {
            var warehouse = await _warehouseService.CreateAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = warehouse.WarehouseId },
                warehouse);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // PUT: api/Warehouse/1
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Update(
        int id,
        UpdateWarehouseDto request)
    {
        try
        {
            var warehouse = await _warehouseService.UpdateAsync(id, request);

            if (warehouse == null)
            {
                return NotFound(new
                {
                    message = "Warehouse not found."
                });
            }

            return Ok(warehouse);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // DELETE: api/Warehouse/1
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            bool deleted = await _warehouseService.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound(new
                {
                    message = "Warehouse not found."
                });
            }

            return Ok(new
            {
                message = "Warehouse deleted successfully."
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}
