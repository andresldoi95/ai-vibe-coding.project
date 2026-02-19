using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.SriConfiguration.Queries.GetSriConfiguration;

/// <summary>
/// Handler for retrieving SRI configuration
/// </summary>
public class GetSriConfigurationQueryHandler : IRequestHandler<GetSriConfigurationQuery, Result<SriConfigurationDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetSriConfigurationQueryHandler> _logger;

    public GetSriConfigurationQueryHandler(
        IUnitOfWork _unitOfWork,
        ITenantContext tenantContext,
        ILogger<GetSriConfigurationQueryHandler> logger)
    {
        this._unitOfWork = _unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<SriConfigurationDto>> Handle(GetSriConfigurationQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_tenantContext.TenantId.HasValue)
            {
                return Result<SriConfigurationDto>.Failure("Tenant context is required");
            }

            var sriConfig = await _unitOfWork.SriConfigurations.GetByTenantIdAsync(_tenantContext.TenantId.Value, cancellationToken);

            if (sriConfig == null)
            {
                // Return empty configuration instead of error
                return Result<SriConfigurationDto>.Success(new SriConfigurationDto
                {
                    Id = Guid.Empty,
                    TenantId = _tenantContext.TenantId.Value,
                    CompanyRuc = string.Empty,
                    LegalName = string.Empty,
                    TradeName = string.Empty,
                    MainAddress = string.Empty,
                    Phone = string.Empty,
                    Email = string.Empty,
                    Environment = Domain.Enums.SriEnvironment.Test,
                    AccountingRequired = false,
                    SpecialTaxpayerNumber = null,
                    IsRiseRegime = false,
                    IsCertificateConfigured = false,
                    IsCertificateValid = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            var sriConfigDto = new SriConfigurationDto
            {
                Id = sriConfig.Id,
                TenantId = sriConfig.TenantId,
                CompanyRuc = sriConfig.CompanyRuc,
                LegalName = sriConfig.LegalName,
                TradeName = sriConfig.TradeName,
                MainAddress = sriConfig.MainAddress,
                Phone = sriConfig.Phone,
                Email = sriConfig.Email,
                Environment = sriConfig.Environment,
                AccountingRequired = sriConfig.AccountingRequired,
                SpecialTaxpayerNumber = sriConfig.SpecialTaxpayerNumber,
                IsRiseRegime = sriConfig.IsRiseRegime,
                IsCertificateConfigured = sriConfig.IsCertificateConfigured,
                CertificateExpiryDate = sriConfig.CertificateExpiryDate,
                IsCertificateValid = sriConfig.IsCertificateValid,
                CreatedAt = sriConfig.CreatedAt,
                UpdatedAt = sriConfig.UpdatedAt
            };

            return Result<SriConfigurationDto>.Success(sriConfigDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving SRI configuration for tenant {TenantId}", _tenantContext.TenantId);
            return Result<SriConfigurationDto>.Failure($"Failed to retrieve SRI configuration: {ex.Message}");
        }
    }
}
