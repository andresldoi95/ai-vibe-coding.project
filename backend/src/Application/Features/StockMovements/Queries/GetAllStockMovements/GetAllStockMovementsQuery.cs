using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.StockMovements.Queries.GetAllStockMovements;

/// <summary>
/// Query to get all stock movements for the current tenant
/// </summary>
public record GetAllStockMovementsQuery : IRequest<Result<List<StockMovementDto>>>;
