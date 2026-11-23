
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInventory.Application.DTOs.PurchaseOrders;
using SmartInventory.Application.Interfaces;

namespace SmartInventory.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PurchaseOrdersController : ControllerBase
{
    private readonly IPurchaseOrderService _service;

    public PurchaseOrdersController(IPurchaseOrderService service)
    {
        _service = service;
    }

    // GET ALL
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    // GET BY ID
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);

        if (result == null)
            return NotFound(new { message = "Purchase order not found." });

        return Ok(result);
    }

    // CREATE
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create(
        CreatePurchaseOrderDto request)
    {
        try
        {
            var result = await _service.CreateAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.PurchaseOrderId },
                result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // UPDATE
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Update(
        int id,
        UpdatePurchaseOrderDto request)
    {
        try
        {
            return Ok(await _service.UpdateAsync(id, request));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // APPROVE
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

    // RECEIVE
    [HttpPost("{id:int}/receive")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Receive(
        int id,
        ReceivePurchaseOrderDto request)
    {
        try
        {
            int? userId = null;

            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdValue, out var parsedUserId))
                userId = parsedUserId;

            return Ok(await _service.ReceiveAsync(
                id, request, userId));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // CANCEL
    [HttpPatch("{id:int}/cancel")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Cancel(int id)
    {
        try
        {
            return Ok(await _service.CancelAsync(id));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}
