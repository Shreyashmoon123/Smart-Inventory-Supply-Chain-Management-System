
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInventory.Application.DTOs.SalesOrders;
using SmartInventory.Application.Interfaces;

namespace SmartInventory.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SalesOrdersController : ControllerBase
{
    private readonly ISalesOrderService _service;

    public SalesOrdersController(ISalesOrderService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _service.GetByIdAsync(id);

        if (order == null)
            return NotFound(new { message = "Sales order not found." });

        return Ok(order);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Staff")]
    public async Task<IActionResult> Create(CreateSalesOrderDto dto)
    {
        try
        {
            var result = await _service.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.SalesOrderId },
                result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:int}/confirm")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Confirm(int id)
    {
        try
        {
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            int? userId = int.TryParse(userIdValue, out var parsedId)
                ? parsedId
                : null;

            return Ok(await _service.ConfirmAsync(id, userId));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateStatus(
        int id, UpdateSalesOrderStatusDto dto)
    {
        try
        {
            return Ok(await _service.UpdateStatusAsync(id, dto.Status));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:int}/cancel")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Cancel(int id)
    {
        try
        {
            await _service.CancelAsync(id);

            return Ok(new
            {
                message = "Sales order cancelled successfully."
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}
