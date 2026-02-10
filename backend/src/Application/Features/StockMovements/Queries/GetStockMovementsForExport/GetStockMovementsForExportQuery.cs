using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.StockMovements.Queries.GetStockMovementsForExport;

/// <summary>
/// Query to get stock movements for export with optional filters
/// </summary>
public record GetStockMovementsForExportQuery : IRequest<Result<List<StockMovementExportDto>>>
{
    /// <summary>
    /// Filter by product brand (optional)
    /// </summary>
    public string? Brand { get; init; }

    /// <summary>
    /// Filter by product category (optional)
    /// </summary>
    public string? Category { get; init; }

    /// <summary>
    /// Filter by warehouse ID (optional)
    /// </summary>
    public Guid? WarehouseId { get; init; }

    /// <summary>
    /// Filter movements from this date (optional)
    /// </summary>
    public DateTime? FromDate { get; init; }

    /// <summary>
    /// Filter movements to this date (optional)
    /// </summary>
    public DateTime? ToDate { get; init; }
}
