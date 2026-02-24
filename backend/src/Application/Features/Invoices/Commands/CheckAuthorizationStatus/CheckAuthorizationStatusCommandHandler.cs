using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs.Sri;
using SaaS.Application.Interfaces;
using SaaS.Domain.Enums;

namespace SaaS.Application.Features.Invoices.Commands.CheckAuthorizationStatus;

public class CheckAuthorizationStatusCommandHandler : IRequestHandler<CheckAuthorizationStatusCommand, Result<SriAuthorizationResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ISriWebServiceClient _sriWebServiceClient;
    private readonly ILogger<CheckAuthorizationStatusCommandHandler> _logger;

    public CheckAuthorizationStatusCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ISriWebServiceClient sriWebServiceClient,
        ILogger<CheckAuthorizationStatusCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _sriWebServiceClient = sriWebServiceClient;
        _logger = logger;
    }

    public async Task<Result<SriAuthorizationResponse>> Handle(CheckAuthorizationStatusCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tenantId = _tenantContext.TenantId ?? throw new UnauthorizedAccessException("Tenant context is not set");

            _logger.LogInformation("Checking authorization status for invoice {InvoiceId}", request.InvoiceId);

            // Get invoice
            var invoice = await _unitOfWork.Invoices.GetByIdAsync(request.InvoiceId, cancellationToken);
            if (invoice == null || invoice.TenantId != tenantId)
            {
                _logger.LogWarning("Invoice {InvoiceId} not found for tenant {TenantId}", request.InvoiceId, tenantId);
                return Result<SriAuthorizationResponse>.Failure("Invoice not found");
            }

            // Validate invoice status
            if (invoice.Status != InvoiceStatus.PendingAuthorization && invoice.Status != InvoiceStatus.Authorized)
            {
                _logger.LogWarning("Invoice {InvoiceId} is not in a valid status to check authorization (current: {Status})", request.InvoiceId, invoice.Status);
                return Result<SriAuthorizationResponse>.Failure($"Invoice cannot check authorization in current status: {invoice.Status}");
            }

            // Validate access key exists
            if (string.IsNullOrEmpty(invoice.AccessKey))
            {
                _logger.LogWarning("Invoice {InvoiceId} has no access key", request.InvoiceId);
                return Result<SriAuthorizationResponse>.Failure("Access key not found. Please regenerate the XML.");
            }

            // Check authorization status from SRI
            var authorizationResponse = await _sriWebServiceClient.CheckAuthorizationAsync(invoice.AccessKey, cancellationToken);

            // Update invoice based on authorization status
            if (authorizationResponse.IsAuthorized && authorizationResponse.AuthorizationNumber != null)
            {
                _logger.LogInformation(
                    "Invoice {InvoiceId} authorized by SRI. Authorization number: {AuthorizationNumber}",
                    request.InvoiceId,
                    authorizationResponse.AuthorizationNumber);

                invoice.Status = InvoiceStatus.Authorized;
                invoice.SriAuthorization = authorizationResponse.AuthorizationNumber;
                invoice.AuthorizationDate = authorizationResponse.AuthorizationDate ?? DateTime.UtcNow;
                invoice.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Invoices.UpdateAsync(invoice);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            else if (authorizationResponse.Status == "EN PROCESAMIENTO")
            {
                _logger.LogInformation("Invoice {InvoiceId} still processing in SRI", request.InvoiceId);
                // Keep status as PendingAuthorization
            }
            else if (!authorizationResponse.IsAuthorized && authorizationResponse.Errors.Any())
            {
                var transientCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                    { "INVALID_RESPONSE", "PARSE_ERROR" };
                var isTransientError = authorizationResponse.Errors.All(e => transientCodes.Contains(e.Code));

                var errorMessages = string.Join("; ", authorizationResponse.Errors.Select(e => $"{e.Code}: {e.Message}"));

                if (isTransientError)
                {
                    // This is a client-side parse / connectivity issue, NOT a definitive SRI rejection.
                    // Keep the invoice in PendingAuthorization so the background job retries.
                    _logger.LogWarning(
                        "Invoice {InvoiceId} received an unreadable response from SRI (will retry). Details: {Errors}",
                        request.InvoiceId,
                        errorMessages);
                }
                else
                {
                    // Real SRI rejection — mark as Rejected and persist the reasons.
                    _logger.LogError(
                        "Invoice {InvoiceId} rejected by SRI. Errors: {Errors}",
                        request.InvoiceId,
                        errorMessages);

                    invoice.Status = InvoiceStatus.Rejected;
                    invoice.UpdatedAt = DateTime.UtcNow;

                    await _unitOfWork.Invoices.UpdateAsync(invoice);

                    foreach (var sriError in authorizationResponse.Errors)
                    {
                        var errorLog = new Domain.Entities.SriErrorLog
                        {
                            InvoiceId = request.InvoiceId,
                            TenantId = invoice.TenantId,
                            Operation = "CheckAuthorization",
                            ErrorCode = sriError.Code,
                            ErrorMessage = sriError.Message,
                            AdditionalData = sriError.AdditionalInfo,
                            OccurredAt = DateTime.UtcNow,
                        };
                        await _unitOfWork.SriErrorLogs.AddAsync(errorLog);
                    }

                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
            }

            _logger.LogInformation(
                "Authorization check completed for invoice {InvoiceId}. Status: {Status}, IsAuthorized: {IsAuthorized}",
                request.InvoiceId,
                authorizationResponse.Status,
                authorizationResponse.IsAuthorized);

            return Result<SriAuthorizationResponse>.Success(authorizationResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking authorization status for invoice {InvoiceId}", request.InvoiceId);

            // Log error to database
            try
            {
                var tenantId = _tenantContext.TenantId ?? Guid.Empty;
                var errorLog = new Domain.Entities.SriErrorLog
                {
                    InvoiceId = request.InvoiceId,
                    TenantId = tenantId,
                    Operation = "CheckAuthorization",
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

            return Result<SriAuthorizationResponse>.Failure($"Error checking authorization: {ex.Message}");
        }
    }
}
