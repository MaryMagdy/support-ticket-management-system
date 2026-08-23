using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportTickets.Application.DTOs;
using SupportTickets.Application.Interfaces;

namespace SupportTickets.Api.Controllers;

/// <summary>
/// Ticket CRUD. Access is restricted per-role: Customers see only their own tickets,
/// SupportAgents see only tickets assigned to them, Admins see everything.
/// </summary>
[ApiController]
[Route("api/tickets")]
[Authorize]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    /// <summary>Paginated, filterable, searchable, sortable ticket list scoped to the caller's role.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] TicketQueryParameters query)
    {
        return Ok(await _ticketService.GetAllAsync(query));
    }

    /// <summary>Get a single ticket by id. Enforces ownership/assignment for non-admins.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        return Ok(await _ticketService.GetByIdAsync(id));
    }

    /// <summary>Create a new ticket (Customer or Admin).</summary>
    [HttpPost]
    public async Task<IActionResult> Create(CreateTicketRequest request)
    {
        var ticket = await _ticketService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, ticket);
    }

    /// <summary>Update ticket fields including status/priority/assignment, with server-side transition validation.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateTicketRequest request)
    {
        return Ok(await _ticketService.UpdateAsync(id, request));
    }

    /// <summary>Delete a ticket (Admin only).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _ticketService.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>Get the activity timeline (status/priority/assignment/comment events) for a ticket.</summary>
    [HttpGet("{id:int}/activity")]
    public async Task<IActionResult> GetActivity(int id)
    {
        return Ok(await _ticketService.GetActivityAsync(id));
    }
}
