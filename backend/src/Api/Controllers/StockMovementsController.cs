using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaS.Application.Features.StockMovements.Commands.CreateStockMovement;
using SaaS.Application.Features.StockMovements.Commands.DeleteStockMovement;
using SaaS.Application.Features.StockMovements.Commands.UpdateStockMovement;
using SaaS.Application.Features.StockMovements.Queries.GetAllStockMovements;
using SaaS.Application.Features.StockMovements.Queries.GetStockMovementById;

namespace SaaS.Api.Controllers;

/// <summary>
/// Controller for managing stock movements
/// </summary>
[ApiController]
[Route("api/v1/stock-movements")]
[Authorize]
public class StockMovementsController : BaseController
{
    public StockMovementsController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Get all stock movements for the current tenant
    /// </summary>
    /// <returns>List of stock movements</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetAllStockMovementsQuery();
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(new { success = false, message = result.Error });
        }

        return Ok(new { success = true, data = result.Value });
    }

    /// <summary>
    /// Get a stock movement by ID
    /// </summary>
    /// <param name="id">Stock movement ID</param>
    /// <returns>Stock movement details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetStockMovementByIdQuery(id);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return NotFound(new { success = false, message = result.Error });
        }

        return Ok(new { success = true, data = result.Value });
    }

    /// <summary>
    /// Create a new stock movement
    /// </summary>
    /// <param name="command">Stock movement creation details</param>
    /// <returns>Created stock movement</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateStockMovementCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { success = false, message = result.Error });
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value!.Id },
            new { success = true, data = result.Value });
    }

    /// <summary>
    /// Update an existing stock movement
    /// </summary>
    /// <param name="id">Stock movement ID</param>
    /// <param name="command">Stock movement update details</param>
    /// <returns>Updated stock movement</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStockMovementCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest(new { success = false, message = "ID mismatch" });
        }

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { success = false, message = result.Error });
        }

        return Ok(new { success = true, data = result.Value });
    }

    /// <summary>
    /// Delete a stock movement (soft delete)
    /// </summary>
    /// <param name="id">Stock movement ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteStockMovementCommand(id);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return NotFound(new { success = false, message = result.Error });
        }

        return NoContent();
    }
}
