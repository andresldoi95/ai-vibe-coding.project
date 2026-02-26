using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs.Sri;
using SaaS.Application.Interfaces;
using SaaS.Domain.Enums;

namespace SaaS.Application.Features.CreditNotes.Commands.CheckCreditNoteAuthorizationStatus;

public class CheckCreditNoteAuthorizationStatusCommandHandler : IRequestHandler<CheckCreditNoteAuthorizationStatusCommand, Result<SriAuthorizationResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ISriWebServiceClient _sriWebServiceClient;
    private readonly ILogger<CheckCreditNoteAuthorizationStatusCommandHandler> _logger;

    public CheckCreditNoteAuthorizationStatusCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ISriWebServiceClient sriWebServiceClient,
        ILogger<CheckCreditNoteAuthorizationStatusCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _sriWebServiceClient = sriWebServiceClient;
        _logger = logger;
    }

    public async Task<Result<SriAuthorizationResponse>> Handle(CheckCreditNoteAuthorizationStatusCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tenantId = _tenantContext.TenantId ?? throw new UnauthorizedAccessException("Tenant context is not set");

            _logger.LogInformation("Checking authorization status for credit note {CreditNoteId}", request.CreditNoteId);

            var creditNote = await _unitOfWork.CreditNotes.GetByIdAsync(request.CreditNoteId, cancellationToken);
            if (creditNote == null || creditNote.TenantId != tenantId)
                return Result<SriAuthorizationResponse>.Failure("Credit note not found");

            if (creditNote.Status != InvoiceStatus.PendingAuthorization && creditNote.Status != InvoiceStatus.Authorized)
                return Result<SriAuthorizationResponse>.Failure($"Credit note cannot check authorization in current status: {creditNote.Status}");

            if (string.IsNullOrEmpty(creditNote.AccessKey))
                return Result<SriAuthorizationResponse>.Failure("Access key not found. Please regenerate the XML.");

            var authorizationResponse = await _sriWebServiceClient.CheckAuthorizationAsync(creditNote.AccessKey, cancellationToken);

            if (authorizationResponse.IsAuthorized && authorizationResponse.AuthorizationNumber != null)
            {
                _logger.LogInformation("Credit note {CreditNoteId} authorized by SRI. Authorization: {AuthNumber}",
                    request.CreditNoteId, authorizationResponse.AuthorizationNumber);

                creditNote.Status = InvoiceStatus.Authorized;
                creditNote.SriAuthorization = authorizationResponse.AuthorizationNumber;
                creditNote.AuthorizationDate = authorizationResponse.AuthorizationDate ?? DateTime.UtcNow;
                creditNote.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.CreditNotes.UpdateAsync(creditNote);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            else if (authorizationResponse.Status == "EN PROCESAMIENTO")
            {
                _logger.LogInformation("Credit note {CreditNoteId} still processing in SRI", request.CreditNoteId);
            }
            else if (!authorizationResponse.IsAuthorized && authorizationResponse.Errors.Any())
            {
                var transientCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "INVALID_RESPONSE", "PARSE_ERROR" };
                var isTransientError = authorizationResponse.Errors.All(e => transientCodes.Contains(e.Code));
                var errorMessages = string.Join("; ", authorizationResponse.Errors.Select(e => $"{e.Code}: {e.Message}"));

                if (isTransientError)
                {
                    _logger.LogWarning("Credit note {CreditNoteId} received an unreadable response from SRI (will retry). Details: {Errors}",
                        request.CreditNoteId, errorMessages);
                }
                else
                {
                    _logger.LogError("Credit note {CreditNoteId} rejected by SRI. Errors: {Errors}", request.CreditNoteId, errorMessages);

                    creditNote.Status = InvoiceStatus.Rejected;
                    creditNote.UpdatedAt = DateTime.UtcNow;

                    await _unitOfWork.CreditNotes.UpdateAsync(creditNote);

                    foreach (var sriError in authorizationResponse.Errors)
                    {
                        var errorLog = new Domain.Entities.SriErrorLog
                        {
                            CreditNoteId = request.CreditNoteId,
                            TenantId = tenantId,
                            Operation = "CheckAuthorization",
                            ErrorCode = sriError.Code,
                            ErrorMessage = sriError.Message,
                            OccurredAt = DateTime.UtcNow
                        };
                        await _unitOfWork.SriErrorLogs.AddAsync(errorLog, cancellationToken);
                    }

                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
            }

            return Result<SriAuthorizationResponse>.Success(authorizationResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking authorization for credit note {CreditNoteId}", request.CreditNoteId);
            return Result<SriAuthorizationResponse>.Failure("An error occurred while checking authorization status");
        }
    }
}
