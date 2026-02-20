using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;

namespace SaaS.Application.Features.TaxRates.Commands.DeleteTaxRate;

public class DeleteTaxRateCommandHandler : IRequestHandler<DeleteTaxRateCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<DeleteTaxRateCommandHandler> _logger;

    public DeleteTaxRateCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<DeleteTaxRateCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteTaxRateCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tenantId = _tenantContext.TenantId ?? throw new UnauthorizedAccessException("Tenant context is not set");

            var taxRate = await _unitOfWork.TaxRates.GetByIdAsync(request.Id, cancellationToken);
            if (taxRate == null || taxRate.TenantId != tenantId)
            {
                return Result<bool>.Failure("Tax rate not found");
            }

            // Soft delete
            taxRate.IsDeleted = true;
            taxRate.DeletedAt = DateTime.UtcNow;

            await _unitOfWork.TaxRates.UpdateAsync(taxRate, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Tax rate {TaxRateId} deleted successfully for tenant {TenantId}", request.Id, tenantId);

            return Result<bool>.Success(true);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access while deleting tax rate {TaxRateId}", request.Id);
            return Result<bool>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tax rate {TaxRateId}", request.Id);
            return Result<bool>.Failure("An error occurred while deleting the tax rate");
        }
    }
}
