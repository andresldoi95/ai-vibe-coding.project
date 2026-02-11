using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.InvoiceConfigurations.Queries.GetInvoiceConfiguration;

public class GetInvoiceConfigurationQueryHandler : IRequestHandler<GetInvoiceConfigurationQuery, Result<InvoiceConfigurationDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetInvoiceConfigurationQueryHandler> _logger;

    public GetInvoiceConfigurationQueryHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<GetInvoiceConfigurationQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<InvoiceConfigurationDto>> Handle(GetInvoiceConfigurationQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var tenantId = _tenantContext.TenantId ?? throw new UnauthorizedAccessException("Tenant context is not set");

            var config = await _unitOfWork.InvoiceConfigurations.GetByTenantAsync(tenantId, cancellationToken);
            if (config == null)
            {
                return Result<InvoiceConfigurationDto>.Failure("Invoice configuration not found for this tenant");
            }

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
            _logger.LogError(ex, "Error retrieving invoice configuration");
            return Result<InvoiceConfigurationDto>.Failure("An error occurred while retrieving the invoice configuration");
        }
    }
}
