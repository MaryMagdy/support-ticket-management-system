using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportTickets.Application.Interfaces;

namespace SupportTickets.Api.Controllers;

/// <summary>
/// Dashboard/analytics endpoints. Admin only.
/// </summary>
[ApiController]
[Route("api/dashboard")]
[Authorize(Roles = "Admin")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>Ticket counts by status, open+critical count, average resolution time, and agent workload.</summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        return Ok(await _dashboardService.GetSummaryAsync());
    }
}
