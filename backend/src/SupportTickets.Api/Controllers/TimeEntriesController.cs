using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportTickets.Application.DTOs;
using SupportTickets.Application.Interfaces;

namespace SupportTickets.Api.Controllers;

/// <summary>
/// Time tracking entries logged against a ticket.
/// </summary>
[ApiController]
[Route("api/tickets/{ticketId:int}/timeentries")]
[Authorize]
public class TimeEntriesController : ControllerBase
{
    private readonly ITimeEntryService _timeEntryService;

    public TimeEntriesController(ITimeEntryService timeEntryService)
    {
        _timeEntryService = timeEntryService;
    }

    /// <summary>Get all time entries for a ticket.</summary>
    [HttpGet]
    public async Task<IActionResult> GetByTicket(int ticketId)
    {
        return Ok(await _timeEntryService.GetByTicketAsync(ticketId));
    }

    /// <summary>Log time spent on a ticket (SupportAgent/Admin only).</summary>
    [HttpPost]
    [Authorize(Roles = "SupportAgent,Admin")]
    public async Task<IActionResult> Add(int ticketId, CreateTimeEntryRequest request)
    {
        var entry = await _timeEntryService.AddAsync(ticketId, request);
        return CreatedAtAction(nameof(GetByTicket), new { ticketId }, entry);
    }
}
