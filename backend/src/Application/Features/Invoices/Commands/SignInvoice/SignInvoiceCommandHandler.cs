using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.Interfaces;
using SaaS.Domain.Enums;

namespace SaaS.Application.Features.Invoices.Commands.SignInvoice;

public class SignInvoiceCommandHandler : IRequestHandler<SignInvoiceCommand, Result<string>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly IXmlSignatureService _xmlSignatureService;
    private readonly ILogger<SignInvoiceCommandHandler> _logger;

    public SignInvoiceCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        IXmlSignatureService xmlSignatureService,
        ILogger<SignInvoiceCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _xmlSignatureService = xmlSignatureService;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(SignInvoiceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tenantId = _tenantContext.TenantId ?? throw new UnauthorizedAccessException("Tenant context is not set");

            _logger.LogInformation("Signing XML for invoice {InvoiceId}", request.InvoiceId);

            // Get invoice
            var invoice = await _unitOfWork.Invoices.GetByIdAsync(request.InvoiceId, cancellationToken);
            if (invoice == null || invoice.TenantId != tenantId)
            {
                _logger.LogWarning("Invoice {InvoiceId} not found for tenant {TenantId}", request.InvoiceId, tenantId);
                return Result<string>.Failure("Invoice not found");
            }

            // Validate invoice status - allow signing from PendingSignature or PendingAuthorization (re-sign after re-generate)
            if (invoice.Status != InvoiceStatus.PendingSignature && invoice.Status != InvoiceStatus.PendingAuthorization)
            {
                _logger.LogWarning("Invoice {InvoiceId} is not in a signable status (current: {Status})", request.InvoiceId, invoice.Status);
                return Result<string>.Failure($"Invoice must be in PendingSignature status to sign (current status: {invoice.Status})");
            }

            // Validate XML file exists
            if (string.IsNullOrEmpty(invoice.XmlFilePath))
            {
                _logger.LogWarning("Invoice {InvoiceId} has no XML file path", request.InvoiceId);
                return Result<string>.Failure("XML file must be generated before signing. Please generate XML first.");
            }

            // Check if XML file exists on disk
            if (!File.Exists(invoice.XmlFilePath))
            {
                _logger.LogWarning("XML file not found at path: {XmlFilePath}", invoice.XmlFilePath);
                return Result<string>.Failure("XML file not found on disk. Please regenerate the XML.");
            }

            // Get SRI configuration with certificate
            var sriConfiguration = await _unitOfWork.SriConfigurations.GetByTenantIdAsync(tenantId, cancellationToken);
            if (sriConfiguration == null)
            {
                _logger.LogWarning("SRI configuration not found for tenant {TenantId}", tenantId);
                return Result<string>.Failure("SRI configuration not found. Please configure SRI settings first.");
            }

            // Validate certificate exists
            if (sriConfiguration.DigitalCertificate == null || sriConfiguration.DigitalCertificate.Length == 0)
            {
                _logger.LogWarning("No certificate found in SRI configuration for tenant {TenantId}", tenantId);
                return Result<string>.Failure("Digital certificate not found. Please upload a certificate in SRI settings.");
            }

            if (string.IsNullOrEmpty(sriConfiguration.CertificatePassword))
            {
                _logger.LogWarning("Certificate password not found in SRI configuration for tenant {TenantId}", tenantId);
                return Result<string>.Failure("Certificate password not configured. Please update SRI settings.");
            }

            // Check certificate expiry
            if (sriConfiguration.CertificateExpiryDate.HasValue)
            {
                if (sriConfiguration.CertificateExpiryDate.Value < DateTime.UtcNow)
                {
                    _logger.LogWarning("Certificate expired on {ExpiryDate} for tenant {TenantId}",
                        sriConfiguration.CertificateExpiryDate.Value, tenantId);
                    return Result<string>.Failure($"Digital certificate expired on {sriConfiguration.CertificateExpiryDate.Value:yyyy-MM-dd}. Please upload a new certificate.");
                }

                // Warn if expiring within 30 days
                if (sriConfiguration.CertificateExpiryDate.Value < DateTime.UtcNow.AddDays(30))
                {
                    _logger.LogWarning("Certificate expiring soon ({ExpiryDate}) for tenant {TenantId}",
                        sriConfiguration.CertificateExpiryDate.Value, tenantId);
                }
            }

            // Sign the XML file
            var signedXmlFilePath = await _xmlSignatureService.SignXmlAsync(
                invoice.XmlFilePath,
                sriConfiguration.DigitalCertificate,
                sriConfiguration.CertificatePassword);

            // Update invoice with signed XML path and change status to PendingAuthorization
            invoice.SignedXmlFilePath = signedXmlFilePath;
            invoice.Status = InvoiceStatus.PendingAuthorization;
            invoice.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Invoices.UpdateAsync(invoice);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "XML signed successfully for invoice {InvoiceId}. Path: {SignedXmlFilePath}",
                request.InvoiceId,
                signedXmlFilePath);

            return Result<string>.Success(signedXmlFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error signing XML for invoice {InvoiceId}", request.InvoiceId);

            // Log error to database
            try
            {
                var tenantId = _tenantContext.TenantId ?? Guid.Empty;
                var errorLog = new Domain.Entities.SriErrorLog
                {
                    InvoiceId = request.InvoiceId,
                    TenantId = tenantId,
                    Operation = "SignDocument",
                    ErrorMessage = ex.Message,
                    StackTrace = ex.StackTrace,
                    OccurredAt = DateTime.UtcNow
                };
                await _unitOfWork.SriErrorLogs.AddAsync(errorLog);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "Failed to log SRI error for invoice {InvoiceId}", request.InvoiceId);
            }

            return Result<string>.Failure($"Error signing XML: {ex.Message}");
        }
    }
}
