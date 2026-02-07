using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Warehouses.Queries.GetAllWarehouses;

/// <summary>
/// Query to get all warehouses for the current tenant
/// </summary>
public record GetAllWarehousesQuery : IRequest<Result<List<WarehouseDto>>>;
