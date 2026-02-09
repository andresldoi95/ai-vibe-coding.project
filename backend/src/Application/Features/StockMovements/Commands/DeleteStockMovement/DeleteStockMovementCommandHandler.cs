using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;

namespace SaaS.Application.Features.StockMovements.Commands.DeleteStockMovement;

/// <summary>
/// Handler for deleting a stock movement (soft delete)
/// </summary>
public class DeleteStockMovementCommandHandler : IRequestHandler<DeleteStockMovementCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<DeleteStockMovementCommandHandler> _logger;

    public DeleteStockMovementCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<DeleteStockMovementCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteStockMovementCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_tenantContext.TenantId.HasValue)
            {
                return Result<bool>.Failure("Tenant context is required");
            }

            var tenantId = _tenantContext.TenantId.Value;

            // Get stock movement
            var stockMovement = await _unitOfWork.StockMovements.GetByIdAsync(request.Id, cancellationToken);
            if (stockMovement == null || stockMovement.TenantId != tenantId)
            {
                return Result<bool>.Failure("Stock movement not found");
            }

            // Soft delete
            stockMovement.IsDeleted = true;
            stockMovement.DeletedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Stock movement {Id} deleted for tenant {TenantId}",
                request.Id,
                tenantId);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting stock movement");
            return Result<bool>.Failure("An error occurred while deleting the stock movement");
        }
    }
}
