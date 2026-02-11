using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaS.Application.Features.EmissionPoints.Commands.CreateEmissionPoint;
using SaaS.Application.Features.EmissionPoints.Commands.DeleteEmissionPoint;
using SaaS.Application.Features.EmissionPoints.Commands.UpdateEmissionPoint;
using SaaS.Application.Features.EmissionPoints.Queries.GetAllEmissionPoints;
using SaaS.Application.Features.EmissionPoints.Queries.GetEmissionPointById;

namespace SaaS.Api.Controllers;

[ApiController]
[Route("api/v1/emission-points")]
[Authorize]
public class EmissionPointsController : BaseController
{
    public EmissionPointsController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Get all emission points for the current tenant
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "emission_points.read")]
    public async Task<IActionResult> GetAll([FromQuery] Guid? establishmentId = null, [FromQuery] bool? isActive = null)
    {
        var query = new GetAllEmissionPointsQuery { EstablishmentId = establishmentId };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error, success = false });
        }

        // Optional filter by isActive in memory if needed
        var data = result.Value;
        if (isActive.HasValue)
        {
            data = data.Where(e => e.IsActive == isActive.Value).ToList();
        }

        return Ok(new { data, success = true });
    }

    /// <summary>
    /// Get an emission point by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "emission_points.read")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetEmissionPointByIdQuery(id);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.Error, success = false });
        }

        return Ok(new { data = result.Value, success = true });
    }

    /// <summary>
    /// Create a new emission point
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "emission_points.create")]
    public async Task<IActionResult> Create([FromBody] CreateEmissionPointCommand command)
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
    /// Update an existing emission point
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "emission_points.update")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmissionPointCommand command)
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
    /// Delete an emission point
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "emission_points.delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteEmissionPointCommand(id);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error, success = false });
        }

        return NoContent();
    }
}
