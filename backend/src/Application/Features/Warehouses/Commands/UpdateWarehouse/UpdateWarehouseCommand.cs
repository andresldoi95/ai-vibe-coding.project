using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Warehouses.Commands.UpdateWarehouse;

/// <summary>
/// Command to update an existing warehouse
/// </summary>
public record UpdateWarehouseCommand : IRequest<Result<WarehouseDto>>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string StreetAddress { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string? State { get; init; }
    public string PostalCode { get; init; } = string.Empty;
    public Guid CountryId { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public bool IsActive { get; init; }
    public decimal? SquareFootage { get; init; }
    public int? Capacity { get; init; }
}
