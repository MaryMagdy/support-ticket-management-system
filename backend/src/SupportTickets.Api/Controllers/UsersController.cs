using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportTickets.Application.DTOs;
using SupportTickets.Application.Interfaces;
using SupportTickets.Domain.Enums;

namespace SupportTickets.Api.Controllers;

/// <summary>
/// User management. Admin only.
/// </summary>
[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>List all users, optionally filtered by role.</summary>
    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetAll([FromQuery] UserRole? role)
    {
        return Ok(await _userService.GetAllAsync(role));
    }

    /// <summary>Get a single user by id.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDto>> GetById(int id)
    {
        return Ok(await _userService.GetByIdAsync(id));
    }

    /// <summary>Create a new user account (e.g. a SupportAgent).</summary>
    [HttpPost]
    public async Task<ActionResult<UserDto>> Create(CreateUserRequest request)
    {
        var user = await _userService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    /// <summary>Update a user's name/role.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserDto>> Update(int id, UpdateUserRequest request)
    {
        return Ok(await _userService.UpdateAsync(id, request));
    }

    /// <summary>Delete a user.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _userService.DeleteAsync(id);
        return NoContent();
    }
}
