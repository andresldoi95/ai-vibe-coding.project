using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Warehouses.Queries.GetWarehouseById;

/// <summary>
/// Query to get a warehouse by ID
/// </summary>
public record GetWarehouseByIdQuery : IRequest<Result<WarehouseDto>>
{
    public Guid Id { get; init; }
}
