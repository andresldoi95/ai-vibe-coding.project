using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Products.Queries.GetAllProducts;

/// <summary>
/// Query to get all products for the current tenant with optional filters
/// </summary>
public record GetAllProductsQuery : IRequest<Result<List<ProductDto>>>
{
    public ProductFilters? Filters { get; init; }
}
