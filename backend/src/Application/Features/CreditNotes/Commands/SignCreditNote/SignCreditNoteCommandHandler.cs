using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.Interfaces;
using SaaS.Domain.Enums;

namespace SaaS.Application.Features.CreditNotes.Commands.SignCreditNote;

public class SignCreditNoteCommandHandler : IRequestHandler<SignCreditNoteCommand, Result<string>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly IXmlSignatureService _xmlSignatureService;
    private readonly ILogger<SignCreditNoteCommandHandler> _logger;

    public SignCreditNoteCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        IXmlSignatureService xmlSignatureService,
        ILogger<SignCreditNoteCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _xmlSignatureService = xmlSignatureService;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(SignCreditNoteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tenantId = _tenantContext.TenantId ?? throw new UnauthorizedAccessException("Tenant context is not set");

            _logger.LogInformation("Signing XML for credit note {CreditNoteId}", request.CreditNoteId);

            var creditNote = await _unitOfWork.CreditNotes.GetByIdAsync(request.CreditNoteId, cancellationToken);
            if (creditNote == null || creditNote.TenantId != tenantId)
                return Result<string>.Failure("Credit note not found");

            if (creditNote.Status != InvoiceStatus.PendingSignature && creditNote.Status != InvoiceStatus.PendingAuthorization)
                return Result<string>.Failure($"Credit note must be in PendingSignature status to sign (current status: {creditNote.Status})");

            if (string.IsNullOrEmpty(creditNote.XmlFilePath))
                return Result<string>.Failure("XML file must be generated before signing. Please generate XML first.");

            if (!File.Exists(creditNote.XmlFilePath))
                return Result<string>.Failure("XML file not found on disk. Please regenerate the XML.");

            var sriConfiguration = await _unitOfWork.SriConfigurations.GetByTenantIdAsync(tenantId, cancellationToken);
            if (sriConfiguration == null)
                return Result<string>.Failure("SRI configuration not found. Please configure SRI settings first.");

            if (sriConfiguration.DigitalCertificate == null || sriConfiguration.DigitalCertificate.Length == 0)
                return Result<string>.Failure("Digital certificate not found. Please upload a certificate in SRI settings.");

            if (string.IsNullOrEmpty(sriConfiguration.CertificatePassword))
                return Result<string>.Failure("Certificate password not configured. Please update SRI settings.");

            if (sriConfiguration.CertificateExpiryDate.HasValue && sriConfiguration.CertificateExpiryDate.Value < DateTime.UtcNow)
                return Result<string>.Failure($"Digital certificate expired on {sriConfiguration.CertificateExpiryDate.Value:yyyy-MM-dd}. Please upload a new certificate.");

            var signedXmlFilePath = await _xmlSignatureService.SignXmlAsync(
                creditNote.XmlFilePath,
                sriConfiguration.DigitalCertificate,
                sriConfiguration.CertificatePassword);

            creditNote.SignedXmlFilePath = signedXmlFilePath;
            creditNote.Status = InvoiceStatus.PendingAuthorization;
            creditNote.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.CreditNotes.UpdateAsync(creditNote);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("XML signed for credit note {CreditNoteId}. Path: {SignedPath}", request.CreditNoteId, signedXmlFilePath);

            return Result<string>.Success(signedXmlFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error signing credit note {CreditNoteId}", request.CreditNoteId);
            return Result<string>.Failure("An error occurred while signing the XML");
        }
    }
}
