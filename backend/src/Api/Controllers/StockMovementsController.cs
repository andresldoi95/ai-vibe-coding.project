using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Features.StockMovements.Commands.CreateStockMovement;
using SaaS.Application.Features.StockMovements.Commands.DeleteStockMovement;
using SaaS.Application.Features.StockMovements.Commands.UpdateStockMovement;
using SaaS.Application.Features.StockMovements.Queries.GetAllStockMovements;
using SaaS.Application.Features.StockMovements.Queries.GetStockMovementById;
using SaaS.Application.Features.StockMovements.Queries.GetStockMovementsForExport;

namespace SaaS.Api.Controllers;

/// <summary>
/// Controller for managing stock movements
/// </summary>
[ApiController]
[Route("api/v1/stock-movements")]
[Authorize]
public class StockMovementsController : BaseController
{
    private readonly IExportService _exportService;

    public StockMovementsController(IMediator mediator, IExportService exportService) : base(mediator)
    {
        _exportService = exportService;
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

    /// <summary>
    /// Export stock movements to Excel or CSV with optional filters
    /// </summary>
    /// <param name="format">Export format: excel or csv (default: excel)</param>
    /// <param name="brand">Filter by product brand (optional)</param>
    /// <param name="category">Filter by product category (optional)</param>
    /// <param name="warehouseId">Filter by warehouse ID (optional)</param>
    /// <param name="fromDate">Filter movements from this date (optional, format: yyyy-MM-dd)</param>
    /// <param name="toDate">Filter movements to this date (optional, format: yyyy-MM-dd)</param>
    [HttpGet("export")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Export(
        [FromQuery] string format = "excel",
        [FromQuery] string? brand = null,
        [FromQuery] string? category = null,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var query = new GetStockMovementsForExportQuery
        {
            Brand = brand,
            Category = category,
            WarehouseId = warehouseId,
            FromDate = fromDate,
            ToDate = toDate
        };

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(new { success = false, message = result.Error });
        }

        var data = result.Value ?? new List<SaaS.Application.DTOs.StockMovementExportDto>();
        var fileName = $"stock-movements-{DateTime.UtcNow:yyyyMMdd-HHmmss}";

        if (format.ToLower() == "csv")
        {
            var csvBytes = _exportService.ExportToCsv(data);
            return File(csvBytes, "text/csv", $"{fileName}.csv");
        }

        var excelBytes = _exportService.ExportToExcel(data, "Stock Movements");
        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{fileName}.xlsx");
    }
}
