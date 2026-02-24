using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.Interfaces;
using SaaS.Domain.Enums;

namespace SaaS.Application.Features.Invoices.Commands.SubmitToSri;

public class SubmitToSriCommandHandler : IRequestHandler<SubmitToSriCommand, Result<string>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ISriWebServiceClient _sriWebServiceClient;
    private readonly ILogger<SubmitToSriCommandHandler> _logger;

    public SubmitToSriCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ISriWebServiceClient sriWebServiceClient,
        ILogger<SubmitToSriCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _sriWebServiceClient = sriWebServiceClient;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(SubmitToSriCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tenantId = _tenantContext.TenantId ?? throw new UnauthorizedAccessException("Tenant context is not set");

            _logger.LogInformation("Submitting invoice {InvoiceId} to SRI", request.InvoiceId);

            // Get invoice
            var invoice = await _unitOfWork.Invoices.GetByIdAsync(request.InvoiceId, cancellationToken);
            if (invoice == null || invoice.TenantId != tenantId)
            {
                _logger.LogWarning("Invoice {InvoiceId} not found for tenant {TenantId}", request.InvoiceId, tenantId);
                return Result<string>.Failure("Invoice not found");
            }

            // Validate invoice status
            if (invoice.Status != InvoiceStatus.PendingAuthorization)
            {
                _logger.LogWarning("Invoice {InvoiceId} is not in PendingAuthorization status (current: {Status})", request.InvoiceId, invoice.Status);

                // If the invoice was previously rejected, fetch the SRI rejection reasons so the user can see them
                if (invoice.Status == InvoiceStatus.Rejected)
                {
                    var rejectionLogs = await _unitOfWork.SriErrorLogs.GetByInvoiceIdAsync(request.InvoiceId, tenantId, cancellationToken);
                    if (rejectionLogs.Any())
                    {
                        var reasons = string.Join("; ", rejectionLogs.Select(e => $"[{e.ErrorCode}] {e.ErrorMessage}"));
                        _logger.LogWarning(
                            "Invoice {InvoiceId} was previously rejected by SRI. Rejection reasons: {Reasons}",
                            request.InvoiceId, reasons);
                        return Result<string>.Failure(
                            $"Invoice was rejected by SRI and cannot be resubmitted without correction. Rejection reasons: {reasons}");
                    }
                }

                return Result<string>.Failure($"Invoice must be in PendingAuthorization status to submit (current status: {invoice.Status})");
            }

            // Validate signed XML file exists
            if (string.IsNullOrEmpty(invoice.SignedXmlFilePath))
            {
                _logger.LogWarning("Invoice {InvoiceId} has no signed XML file path", request.InvoiceId);
                return Result<string>.Failure("Signed XML file not found. Please generate and sign the XML first.");
            }

            // Check if signed XML file exists on disk
            if (!File.Exists(invoice.SignedXmlFilePath))
            {
                _logger.LogWarning("Signed XML file not found at path: {SignedXmlFilePath}", invoice.SignedXmlFilePath);
                return Result<string>.Failure("Signed XML file not found on disk. Please regenerate and sign the XML.");
            }

            // Read signed XML content
            var signedXmlContent = await File.ReadAllTextAsync(invoice.SignedXmlFilePath, cancellationToken);

            // Submit document to SRI
            var submissionResponse = await _sriWebServiceClient.SubmitDocumentAsync(signedXmlContent, cancellationToken);

            if (!submissionResponse.IsSuccess)
            {
                var errorMessages = string.Join("; ", submissionResponse.Errors.Select(e => $"{e.Code}: {e.Message}"));
                _logger.LogError(
                    "Failed to submit invoice {InvoiceId} to SRI. Errors: {Errors}",
                    request.InvoiceId,
                    errorMessages);

                return Result<string>.Failure($"SRI submission failed: {errorMessages}");
            }

            // Update invoice - keep status as PendingAuthorization (will be updated by background job when checking authorization)
            invoice.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Invoices.UpdateAsync(invoice);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Invoice {InvoiceId} submitted successfully to SRI. Message: {Message}",
                request.InvoiceId,
                submissionResponse.Message);

            return Result<string>.Success($"Document submitted successfully to SRI. Status: {submissionResponse.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting invoice {InvoiceId} to SRI", request.InvoiceId);

            // Log error to database
            try
            {
                var tenantId = _tenantContext.TenantId ?? Guid.Empty;
                var errorLog = new Domain.Entities.SriErrorLog
                {
                    InvoiceId = request.InvoiceId,
                    TenantId = tenantId,
                    Operation = "SubmitToSRI",
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

            return Result<string>.Failure($"Error submitting to SRI: {ex.Message}");
        }
    }
}
