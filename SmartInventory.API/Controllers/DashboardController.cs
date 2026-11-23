
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInventory.Application.Interfaces;

namespace SmartInventory.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        return Ok(await _dashboardService.GetSummaryAsync());
    }

    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStock()
    {
        return Ok(await _dashboardService.GetLowStockAsync());
    }

    [HttpGet("recent-transactions")]
    public async Task<IActionResult> GetRecentTransactions()
    {
        return Ok(await _dashboardService.GetRecentTransactionsAsync());
    }
}
