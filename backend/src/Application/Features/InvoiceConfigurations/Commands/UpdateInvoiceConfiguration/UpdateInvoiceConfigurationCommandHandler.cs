using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.InvoiceConfigurations.Commands.UpdateInvoiceConfiguration;

public class UpdateInvoiceConfigurationCommandHandler : IRequestHandler<UpdateInvoiceConfigurationCommand, Result<InvoiceConfigurationDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<UpdateInvoiceConfigurationCommandHandler> _logger;

    public UpdateInvoiceConfigurationCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<UpdateInvoiceConfigurationCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<InvoiceConfigurationDto>> Handle(UpdateInvoiceConfigurationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tenantId = _tenantContext.TenantId ?? throw new UnauthorizedAccessException("Tenant context is not set");

            var config = await _unitOfWork.InvoiceConfigurations.GetByTenantAsync(tenantId, cancellationToken);
            if (config == null)
            {
                return Result<InvoiceConfigurationDto>.Failure("Invoice configuration not found for this tenant");
            }

            config.EstablishmentCode = request.EstablishmentCode;
            config.EmissionPointCode = request.EmissionPointCode;
            config.DefaultTaxRateId = request.DefaultTaxRateId;
            config.DefaultWarehouseId = request.DefaultWarehouseId;
            config.DueDays = request.DueDays;
            config.RequireCustomerTaxId = request.RequireCustomerTaxId;

            await _unitOfWork.InvoiceConfigurations.UpdateAsync(config, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Invoice configuration updated successfully for tenant {TenantId}", tenantId);

            var configDto = new InvoiceConfigurationDto
            {
                Id = config.Id,
                TenantId = config.TenantId,
                EstablishmentCode = config.EstablishmentCode,
                EmissionPointCode = config.EmissionPointCode,
                NextSequentialNumber = config.NextSequentialNumber,
                DefaultTaxRateId = config.DefaultTaxRateId,
                DefaultWarehouseId = config.DefaultWarehouseId,
                DueDays = config.DueDays,
                RequireCustomerTaxId = config.RequireCustomerTaxId,
                CreatedAt = config.CreatedAt,
                UpdatedAt = config.UpdatedAt
            };

            return Result<InvoiceConfigurationDto>.Success(configDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating invoice configuration");
            return Result<InvoiceConfigurationDto>.Failure("An error occurred while updating the invoice configuration");
        }
    }
}
