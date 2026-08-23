using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportTickets.Application.DTOs;
using SupportTickets.Application.Interfaces;

namespace SupportTickets.Api.Controllers;

/// <summary>
/// Comments on tickets. Any role with access to the ticket may view/add comments.
/// </summary>
[ApiController]
[Route("api/tickets/{ticketId:int}/comments")]
[Authorize]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    /// <summary>Get all comments for a ticket.</summary>
    [HttpGet]
    public async Task<IActionResult> GetByTicket(int ticketId)
    {
        return Ok(await _commentService.GetByTicketAsync(ticketId));
    }

    /// <summary>Add a comment to a ticket.</summary>
    [HttpPost]
    public async Task<IActionResult> Add(int ticketId, CreateCommentRequest request)
    {
        var comment = await _commentService.AddAsync(ticketId, request);
        return CreatedAtAction(nameof(GetByTicket), new { ticketId }, comment);
    }
}
