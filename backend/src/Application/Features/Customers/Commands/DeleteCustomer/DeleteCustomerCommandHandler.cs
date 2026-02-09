using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;

namespace SaaS.Application.Features.Customers.Commands.DeleteCustomer;

/// <summary>
/// Handler for deleting a customer (soft delete)
/// </summary>
public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<DeleteCustomerCommandHandler> _logger;

    public DeleteCustomerCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<DeleteCustomerCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_tenantContext.TenantId.HasValue)
            {
                return Result<bool>.Failure("Tenant context is required");
            }

            var customer = await _unitOfWork.Customers.GetByIdAsync(request.Id, cancellationToken);

            if (customer == null)
            {
                return Result<bool>.Failure("Customer not found");
            }

            // Verify customer belongs to the current tenant
            if (customer.TenantId != _tenantContext.TenantId.Value)
            {
                return Result<bool>.Failure("Unauthorized access to customer");
            }

            // Soft delete: Mark as deleted instead of removing from database
            customer.IsDeleted = true;
            customer.DeletedAt = DateTime.UtcNow;
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Customer {Id} soft deleted successfully for tenant {TenantId}",
                customer.Id,
                customer.TenantId);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer {Id}", request.Id);
            return Result<bool>.Failure("An error occurred while deleting the customer");
        }
    }
}
