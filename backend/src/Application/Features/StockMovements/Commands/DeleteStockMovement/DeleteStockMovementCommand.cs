using MediatR;
using SaaS.Application.Common.Models;

namespace SaaS.Application.Features.StockMovements.Commands.DeleteStockMovement;

/// <summary>
/// Command to delete a stock movement (soft delete)
/// </summary>
public record DeleteStockMovementCommand(Guid Id) : IRequest<Result<bool>>;
