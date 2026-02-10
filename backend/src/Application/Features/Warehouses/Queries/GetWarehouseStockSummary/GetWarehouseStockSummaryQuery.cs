using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Warehouses.Queries.GetWarehouseStockSummary;

/// <summary>
/// Query to get warehouse stock summary for export
/// Returns aggregated stock information per warehouse and product
/// </summary>
public record GetWarehouseStockSummaryQuery : IRequest<Result<List<WarehouseStockSummaryDto>>>;
