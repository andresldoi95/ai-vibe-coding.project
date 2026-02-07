using MediatR;
using SaaS.Application.Common.Models;

namespace SaaS.Application.Features.Warehouses.Commands.DeleteWarehouse;

/// <summary>
/// Command to delete (soft delete) a warehouse
/// </summary>
public record DeleteWarehouseCommand : IRequest<Result<bool>>
{
    public Guid Id { get; init; }
}
