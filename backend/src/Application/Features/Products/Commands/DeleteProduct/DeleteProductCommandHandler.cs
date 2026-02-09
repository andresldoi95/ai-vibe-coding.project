using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;

namespace SaaS.Application.Features.Products.Commands.DeleteProduct;

/// <summary>
/// Handler for deleting a product (soft delete)
/// </summary>
public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<DeleteProductCommandHandler> _logger;

    public DeleteProductCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<DeleteProductCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_tenantContext.TenantId.HasValue)
            {
                return Result<bool>.Failure("Tenant context is required");
            }

            var product = await _unitOfWork.Products.GetByIdAsync(request.Id, cancellationToken);

            if (product == null)
            {
                return Result<bool>.Failure("Product not found");
            }

            // Verify tenant ownership
            if (product.TenantId != _tenantContext.TenantId.Value)
            {
                return Result<bool>.Failure("Product not found");
            }

            // Soft delete
            product.IsDeleted = true;
            product.DeletedAt = DateTime.UtcNow;

            _unitOfWork.Products.Update(product);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Product {Code} deleted successfully for tenant {TenantId}",
                product.Code,
                product.TenantId);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product");
            return Result<bool>.Failure("An error occurred while deleting the product");
        }
    }
}
