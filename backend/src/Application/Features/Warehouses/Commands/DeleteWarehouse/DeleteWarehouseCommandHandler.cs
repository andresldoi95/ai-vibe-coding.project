using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;

namespace SaaS.Application.Features.Warehouses.Commands.DeleteWarehouse;

/// <summary>
/// Handler for deleting (soft delete) a warehouse
/// </summary>
public class DeleteWarehouseCommandHandler : IRequestHandler<DeleteWarehouseCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<DeleteWarehouseCommandHandler> _logger;

    public DeleteWarehouseCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<DeleteWarehouseCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteWarehouseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_tenantContext.TenantId.HasValue)
            {
                return Result<bool>.Failure("Tenant context is required");
            }

            // Get existing warehouse
            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(request.Id, cancellationToken);

            if (warehouse == null || warehouse.IsDeleted)
            {
                return Result<bool>.Failure($"Warehouse with ID '{request.Id}' not found");
            }

            // Verify warehouse belongs to current tenant
            if (warehouse.TenantId != _tenantContext.TenantId.Value)
            {
                return Result<bool>.Failure("Access denied");
            }

            // Soft delete
            await _unitOfWork.Warehouses.DeleteAsync(warehouse, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Warehouse {Id} deleted successfully for tenant {TenantId}",
                warehouse.Id,
                warehouse.TenantId);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting warehouse {Id}", request.Id);
            return Result<bool>.Failure("An error occurred while deleting the warehouse");
        }
    }
}
