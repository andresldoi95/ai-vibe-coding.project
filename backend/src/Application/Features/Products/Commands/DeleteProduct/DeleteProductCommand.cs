using MediatR;
using SaaS.Application.Common.Models;

namespace SaaS.Application.Features.Products.Commands.DeleteProduct;

/// <summary>
/// Command to delete a product (soft delete)
/// </summary>
public record DeleteProductCommand : IRequest<Result<bool>>
{
    public Guid Id { get; init; }
}
