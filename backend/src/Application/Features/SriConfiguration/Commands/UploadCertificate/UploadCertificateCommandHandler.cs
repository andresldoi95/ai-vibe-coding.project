using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;
using System.Security.Cryptography.X509Certificates;

namespace SaaS.Application.Features.SriConfiguration.Commands.UploadCertificate;

/// <summary>
/// Handler for uploading digital certificate
/// </summary>
public class UploadCertificateCommandHandler : IRequestHandler<UploadCertificateCommand, Result<SriConfigurationDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<UploadCertificateCommandHandler> _logger;

    public UploadCertificateCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<UploadCertificateCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<SriConfigurationDto>> Handle(UploadCertificateCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_tenantContext.TenantId.HasValue)
            {
                return Result<SriConfigurationDto>.Failure("Tenant context is required");
            }

            // Validate certificate
            try
            {
                var certificate = new X509Certificate2(request.CertificateFile, request.CertificatePassword);
                if (certificate.NotAfter < DateTime.UtcNow)
                {
                    return Result<SriConfigurationDto>.Failure("Certificate has expired");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Invalid certificate or password for tenant {TenantId}", _tenantContext.TenantId);
                return Result<SriConfigurationDto>.Failure("Invalid certificate or password");
            }

            var sriConfig = await _unitOfWork.SriConfigurations.GetByTenantIdAsync(_tenantContext.TenantId.Value, cancellationToken);

            if (sriConfig == null)
            {
                return Result<SriConfigurationDto>.Failure("SRI configuration not found. Please configure company information first.");
            }

            // Store certificate and password (password should be encrypted in production)
            sriConfig.DigitalCertificate = request.CertificateFile;
            sriConfig.CertificatePassword = request.CertificatePassword; // TODO: Encrypt password

            await _unitOfWork.SriConfigurations.UpdateAsync(sriConfig, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Digital certificate uploaded successfully for tenant {TenantId}",
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
            _logger.LogError(ex, "Error uploading certificate for tenant {TenantId}", _tenantContext.TenantId);
            return Result<SriConfigurationDto>.Failure($"Failed to upload certificate: {ex.Message}");
        }
    }
}
