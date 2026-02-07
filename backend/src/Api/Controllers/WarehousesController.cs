using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaS.Application.Features.Warehouses.Commands.CreateWarehouse;
using SaaS.Application.Features.Warehouses.Commands.DeleteWarehouse;
using SaaS.Application.Features.Warehouses.Commands.UpdateWarehouse;
using SaaS.Application.Features.Warehouses.Queries.GetAllWarehouses;
using SaaS.Application.Features.Warehouses.Queries.GetWarehouseById;

namespace SaaS.Api.Controllers;

[ApiController]
[Route("api/v1/warehouses")]
[Authorize]
public class WarehousesController : BaseController
{
    public WarehousesController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Get all warehouses for the current tenant
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetAllWarehousesQuery();
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error, success = false });
        }

        return Ok(new { data = result.Value, success = true });
    }

    /// <summary>
    /// Get a warehouse by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetWarehouseByIdQuery { Id = id };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.Error, success = false });
        }

        return Ok(new { data = result.Value, success = true });
    }

    /// <summary>
    /// Create a new warehouse
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWarehouseCommand command)
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
    /// Update an existing warehouse
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWarehouseCommand command)
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
    /// Delete a warehouse (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteWarehouseCommand { Id = id };
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error, success = false });
        }

        return Ok(new { message = "Warehouse deleted successfully", success = true });
    }
}
