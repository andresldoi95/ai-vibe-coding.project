using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Products.Queries.GetProductById;

/// <summary>
/// Query to get a product by ID
/// </summary>
public record GetProductByIdQuery : IRequest<Result<ProductDto>>
{
    public Guid Id { get; init; }
}
