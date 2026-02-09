using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaS.Application.Features.Roles.Commands.CreateRole;
using SaaS.Application.Features.Roles.Commands.DeleteRole;
using SaaS.Application.Features.Roles.Commands.UpdateRole;
using SaaS.Application.Features.Roles.Queries.GetAllRoles;
using SaaS.Application.Features.Roles.Queries.GetRoleById;

namespace SaaS.Api.Controllers;

[ApiController]
[Route("api/v1/roles")]
[Authorize]
public class RolesController : BaseController
{
    public RolesController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Get all roles for the current tenant
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "roles.read")]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetAllRolesQuery();
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error, success = false });
        }

        return Ok(new { data = result.Value, success = true });
    }

    /// <summary>
    /// Get a role by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "roles.read")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetRoleByIdQuery { Id = id };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.Error, success = false });
        }

        return Ok(new { data = result.Value, success = true });
    }

    /// <summary>
    /// Create a new custom role
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "roles.manage")]
    public async Task<IActionResult> Create([FromBody] CreateRoleCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.IsSuccess || result.Value == null)
        {
            return BadRequest(new { message = result.Error, success = false });
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value.Id },
            new { data = result.Value, success = true });
    }

    /// <summary>
    /// Update an existing role
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "roles.manage")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoleCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest(new { message = "ID mismatch", success = false });
        }

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error, success = false });
        }

        return Ok(new { data = result.Value, success = true });
    }

    /// <summary>
    /// Delete a custom role (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "roles.manage")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteRoleCommand { Id = id };
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error, success = false });
        }

        return Ok(new { message = "Role deleted successfully", success = true });
    }
}
