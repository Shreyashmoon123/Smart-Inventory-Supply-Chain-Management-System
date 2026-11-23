
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInventory.Application.DTOs.AuditLogs;
using SmartInventory.Application.Interfaces;

namespace SmartInventory.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Manager")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogsController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? userId,
        [FromQuery] string? action,
        [FromQuery] string? entityName,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        if (fromDate.HasValue && toDate.HasValue &&
            fromDate.Value.Date > toDate.Value.Date)
        {
            return BadRequest("From date cannot be greater than to date.");
        }

        var logs = await _auditLogService.GetAllAsync(
            userId, action, entityName, fromDate, toDate);

        return Ok(logs);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var log = await _auditLogService.GetByIdAsync(id);

        if (log == null)
            return NotFound("Audit log not found.");

        return Ok(log);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateAuditLogDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        int? userId = int.TryParse(userIdClaim, out var parsedUserId)
            ? parsedUserId
            : null;

        try
        {
            var result = await _auditLogService.CreateAsync(dto, userId);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.AuditLogId },
                result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
