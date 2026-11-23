
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInventory.Application.DTOs.Payments;
using SmartInventory.Application.Interfaces;

namespace SmartInventory.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _service;

    public PaymentsController(IPaymentService service)
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
        var payment = await _service.GetByIdAsync(id);

        if (payment == null)
            return NotFound(new { message = "Payment not found." });

        return Ok(payment);
    }

    [HttpGet("sales-order/{salesOrderId:int}")]
    public async Task<IActionResult> GetBySalesOrder(int salesOrderId)
    {
        try
        {
            return Ok(await _service.GetBySalesOrderIdAsync(salesOrderId));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("sales-order/{salesOrderId:int}/summary")]
    public async Task<IActionResult> GetSummary(int salesOrderId)
    {
        try
        {
            return Ok(await _service.GetSummaryAsync(salesOrderId));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Staff")]
    public async Task<IActionResult> Create(CreatePaymentDto dto)
    {
        try
        {
            var payment = await _service.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = payment.PaymentId },
                payment);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateStatus(
        int id, UpdatePaymentStatusDto dto)
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
}
