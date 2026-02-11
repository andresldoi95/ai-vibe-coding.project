using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.SriConfiguration.Commands.UpdateSriConfiguration;

/// <summary>
/// Handler for updating SRI configuration
/// </summary>
public class UpdateSriConfigurationCommandHandler : IRequestHandler<UpdateSriConfigurationCommand, Result<SriConfigurationDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<UpdateSriConfigurationCommandHandler> _logger;

    public UpdateSriConfigurationCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<UpdateSriConfigurationCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<SriConfigurationDto>> Handle(UpdateSriConfigurationCommand request, CancellationToken cancellationToken)
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
                // Create new configuration if it doesn't exist
                sriConfig = new Domain.Entities.SriConfiguration
                {
                    TenantId = _tenantContext.TenantId.Value
                };
                await _unitOfWork.SriConfigurations.AddAsync(sriConfig, cancellationToken);
            }

            // Update configuration
            sriConfig.CompanyRuc = request.CompanyRuc;
            sriConfig.LegalName = request.LegalName;
            sriConfig.TradeName = request.TradeName;
            sriConfig.MainAddress = request.MainAddress;
            sriConfig.Environment = request.Environment;
            sriConfig.AccountingRequired = request.AccountingRequired;

            if (sriConfig.Id != Guid.Empty)
            {
                await _unitOfWork.SriConfigurations.UpdateAsync(sriConfig, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "SRI configuration updated successfully for tenant {TenantId}",
                sriConfig.TenantId);

            // Map to DTO
            var sriConfigDto = new SriConfigurationDto
            {
                Id = sriConfig.Id,
                TenantId = sriConfig.TenantId,
                CompanyRuc = sriConfig.CompanyRuc,
                LegalName = sriConfig.LegalName,
                TradeName = sriConfig.TradeName,
                MainAddress = sriConfig.MainAddress,
                Environment = sriConfig.Environment,
                AccountingRequired = sriConfig.AccountingRequired,
                IsCertificateConfigured = sriConfig.IsCertificateConfigured,
                IsCertificateValid = sriConfig.IsCertificateValid,
                CreatedAt = sriConfig.CreatedAt,
                UpdatedAt = sriConfig.UpdatedAt
            };

            return Result<SriConfigurationDto>.Success(sriConfigDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating SRI configuration for tenant {TenantId}", _tenantContext.TenantId);
            return Result<SriConfigurationDto>.Failure($"Failed to update SRI configuration: {ex.Message}");
        }
    }
}
