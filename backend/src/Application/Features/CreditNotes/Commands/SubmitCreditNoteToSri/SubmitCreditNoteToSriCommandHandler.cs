using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.Interfaces;
using SaaS.Domain.Enums;

namespace SaaS.Application.Features.CreditNotes.Commands.SubmitCreditNoteToSri;

public class SubmitCreditNoteToSriCommandHandler : IRequestHandler<SubmitCreditNoteToSriCommand, Result<string>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ISriWebServiceClient _sriWebServiceClient;
    private readonly ILogger<SubmitCreditNoteToSriCommandHandler> _logger;

    public SubmitCreditNoteToSriCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ISriWebServiceClient sriWebServiceClient,
        ILogger<SubmitCreditNoteToSriCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _sriWebServiceClient = sriWebServiceClient;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(SubmitCreditNoteToSriCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tenantId = _tenantContext.TenantId ?? throw new UnauthorizedAccessException("Tenant context is not set");

            _logger.LogInformation("Submitting credit note {CreditNoteId} to SRI", request.CreditNoteId);

            var creditNote = await _unitOfWork.CreditNotes.GetByIdAsync(request.CreditNoteId, cancellationToken);
            if (creditNote == null || creditNote.TenantId != tenantId)
                return Result<string>.Failure("Credit note not found");

            if (creditNote.Status != InvoiceStatus.PendingAuthorization)
            {
                if (creditNote.Status == InvoiceStatus.Rejected)
                {
                    var rejectionLogs = await _unitOfWork.SriErrorLogs.GetByCreditNoteIdAsync(request.CreditNoteId, tenantId, cancellationToken);
                    if (rejectionLogs.Any())
                    {
                        var reasons = string.Join("; ", rejectionLogs.Select(e => $"[{e.ErrorCode}] {e.ErrorMessage}"));
                        return Result<string>.Failure($"Credit note was rejected by SRI and cannot be resubmitted without correction. Rejection reasons: {reasons}");
                    }
                }
                return Result<string>.Failure($"Credit note must be in PendingAuthorization status to submit (current status: {creditNote.Status})");
            }

            if (string.IsNullOrEmpty(creditNote.SignedXmlFilePath))
                return Result<string>.Failure("Signed XML file not found. Please generate and sign the XML first.");

            if (!File.Exists(creditNote.SignedXmlFilePath))
                return Result<string>.Failure("Signed XML file not found on disk. Please regenerate and sign the XML.");

            var signedXmlContent = await File.ReadAllTextAsync(creditNote.SignedXmlFilePath, cancellationToken);

            var submissionResponse = await _sriWebServiceClient.SubmitDocumentAsync(signedXmlContent, cancellationToken);

            if (!submissionResponse.IsSuccess)
            {
                var errorMessages = string.Join("; ", submissionResponse.Errors.Select(e => $"{e.Code}: {e.Message}"));
                _logger.LogError("Failed to submit credit note {CreditNoteId} to SRI. Errors: {Errors}", request.CreditNoteId, errorMessages);
                return Result<string>.Failure($"SRI submission failed: {errorMessages}");
            }

            creditNote.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.CreditNotes.UpdateAsync(creditNote);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Credit note {CreditNoteId} submitted to SRI. Message: {Message}", request.CreditNoteId, submissionResponse.Message);

            return Result<string>.Success($"Document submitted successfully to SRI. Status: {submissionResponse.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting credit note {CreditNoteId} to SRI", request.CreditNoteId);

            try
            {
                var tenantId = _tenantContext.TenantId ?? Guid.Empty;
                var errorLog = new Domain.Entities.SriErrorLog
                {
                    CreditNoteId = request.CreditNoteId,
                    TenantId = tenantId,
                    Operation = "SubmitToSri",
                    ErrorCode = "SOAP_ERROR",
                    ErrorMessage = ex.Message,
                    StackTrace = ex.StackTrace,
                    OccurredAt = DateTime.UtcNow
                };
                await _unitOfWork.SriErrorLogs.AddAsync(errorLog, CancellationToken.None);
                await _unitOfWork.SaveChangesAsync(CancellationToken.None);
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "Failed to log SRI error for credit note {CreditNoteId}", request.CreditNoteId);
            }

            return Result<string>.Failure("An error occurred while submitting the credit note to SRI");
        }
    }
}
