using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaS.Application.Features.Users.Commands.InviteUser;
using SaaS.Application.Features.Users.Commands.RemoveUserFromCompany;
using SaaS.Application.Features.Users.Commands.UpdateUserRole;
using SaaS.Application.Features.Users.Queries.GetCompanyUsers;
using SaaS.Application.Features.Users.Queries.GetUserById;
using SaaS.Application.DTOs;

namespace SaaS.Api.Controllers;

/// <summary>
/// User management endpoints
/// </summary>
[ApiController]
[Route("api/v1/users")]
[Authorize]
public class UsersController : BaseController
{
    public UsersController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Get all users in the current company
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "users.read")]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetCompanyUsersQuery();
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error, success = false });
        }

        return Ok(new { data = result.Value, success = true });
    }

    /// <summary>
    /// Get a specific user by ID in the current company
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "users.read")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetUserByIdQuery(id);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.Error, success = false });
        }

        return Ok(new { data = result.Value, success = true });
    }

    /// <summary>
    /// Invite a user to the current company
    /// </summary>
    [HttpPost("invite")]
    [Authorize(Policy = "users.invite")]
    public async Task<IActionResult> Invite([FromBody] InviteUserDto dto)
    {
        var command = new InviteUserCommand
        {
            Email = dto.Email,
            RoleId = dto.RoleId,
            PersonalMessage = dto.PersonalMessage
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error, success = false });
        }

        return Ok(new { message = "Invitation sent successfully", success = true });
    }

    /// <summary>
    /// Update a user's role in the current company
    /// </summary>
    [HttpPut("{id:guid}/role")]
    [Authorize(Policy = "users.update")]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateUserRoleDto dto)
    {
        var command = new UpdateUserRoleCommand
        {
            UserId = id,
            NewRoleId = dto.NewRoleId
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error, success = false });
        }

        return Ok(new { message = "User role updated successfully", success = true });
    }

    /// <summary>
    /// Remove a user from the current company
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "users.remove")]
    public async Task<IActionResult> Remove(Guid id)
    {
        var command = new RemoveUserFromCompanyCommand(id);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error, success = false });
        }

        return Ok(new { message = "User removed from company successfully", success = true });
    }
}
