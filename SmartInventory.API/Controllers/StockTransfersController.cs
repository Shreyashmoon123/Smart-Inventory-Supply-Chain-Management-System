
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInventory.Application.DTOs.StockTransfers;
using SmartInventory.Application.Interfaces;

namespace SmartInventory.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StockTransfersController : ControllerBase
{
    private readonly IStockTransferService _service;

    public StockTransfersController(IStockTransferService service)
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
        var transfer = await _service.GetByIdAsync(id);

        if (transfer == null)
            return NotFound(new { message = "Transfer not found." });

        return Ok(transfer);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Staff")]
    public async Task<IActionResult> Create(CreateStockTransferDto dto)
    {
        try
        {
            var result = await _service.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.StockTransferId },
                result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:int}/approve")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Approve(int id)
    {
        try
        {
            return Ok(await _service.ApproveAsync(id));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:int}/dispatch")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Dispatch(int id)
    {
        try
        {
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            int? userId = int.TryParse(userIdValue, out var parsedId)
                ? parsedId
                : null;

            return Ok(await _service.DispatchAsync(id, userId));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:int}/complete")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Complete(int id)
    {
        try
        {
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            int? userId = int.TryParse(userIdValue, out var parsedId)
                ? parsedId
                : null;

            return Ok(await _service.CompleteAsync(id, userId));
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
                message = "Stock transfer cancelled successfully."
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}
