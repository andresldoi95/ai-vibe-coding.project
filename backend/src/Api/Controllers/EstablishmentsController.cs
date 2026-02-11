using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaS.Application.Features.Establishments.Commands.CreateEstablishment;
using SaaS.Application.Features.Establishments.Commands.DeleteEstablishment;
using SaaS.Application.Features.Establishments.Commands.UpdateEstablishment;
using SaaS.Application.Features.Establishments.Queries.GetAllEstablishments;
using SaaS.Application.Features.Establishments.Queries.GetEstablishmentById;

namespace SaaS.Api.Controllers;

[ApiController]
[Route("api/v1/establishments")]
[Authorize]
public class EstablishmentsController : BaseController
{
    public EstablishmentsController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Get all establishments for the current tenant
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "establishments.read")]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetAllEstablishmentsQuery();
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error, success = false });
        }

        return Ok(new { data = result.Value, success = true });
    }

    /// <summary>
    /// Get an establishment by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "establishments.read")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetEstablishmentByIdQuery(id);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.Error, success = false });
        }

        return Ok(new { data = result.Value, success = true });
    }

    /// <summary>
    /// Create a new establishment
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "establishments.create")]
    public async Task<IActionResult> Create([FromBody] CreateEstablishmentCommand command)
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
    /// Update an existing establishment
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "establishments.update")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEstablishmentCommand command)
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
    /// Delete an establishment
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "establishments.delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteEstablishmentCommand(id);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error, success = false });
        }

        return NoContent();
    }
}
