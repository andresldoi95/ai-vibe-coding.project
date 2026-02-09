using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Products.Queries.GetProductInventory;

/// <summary>
/// Query to get warehouse inventory for a specific product
/// </summary>
public record GetProductInventoryQuery(Guid ProductId) : IRequest<Result<List<WarehouseInventoryDto>>>;
