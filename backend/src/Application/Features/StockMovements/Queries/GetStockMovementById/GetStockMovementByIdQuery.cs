using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.StockMovements.Queries.GetStockMovementById;

/// <summary>
/// Query to get a stock movement by ID
/// </summary>
public record GetStockMovementByIdQuery(Guid Id) : IRequest<Result<StockMovementDto>>;
