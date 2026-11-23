
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInventory.Application.Interfaces;

namespace SmartInventory.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("sales")]
    public async Task<IActionResult> GetSalesReport(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
            return BadRequest("From date cannot be greater than to date.");

        return Ok(await _reportService.GetSalesReportAsync(fromDate, toDate));
    }

    [HttpGet("purchases")]
    public async Task<IActionResult> GetPurchaseReport(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
            return BadRequest("From date cannot be greater than to date.");

        return Ok(await _reportService.GetPurchaseReportAsync(fromDate, toDate));
    }

    [HttpGet("inventory")]
    public async Task<IActionResult> GetInventoryReport()
    {
        return Ok(await _reportService.GetInventoryReportAsync());
    }

    [HttpGet("stock-transactions")]
    public async Task<IActionResult> GetStockTransactionReport(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
            return BadRequest("From date cannot be greater than to date.");

        return Ok(await _reportService.GetStockTransactionReportAsync(fromDate, toDate));
    }
}
