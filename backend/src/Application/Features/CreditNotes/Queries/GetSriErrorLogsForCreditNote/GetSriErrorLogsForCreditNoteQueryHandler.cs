using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.CreditNotes.Queries.GetSriErrorLogsForCreditNote;

public class GetSriErrorLogsForCreditNoteQueryHandler
    : IRequestHandler<GetSriErrorLogsForCreditNoteQuery, Result<List<SriErrorLogDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetSriErrorLogsForCreditNoteQueryHandler> _logger;

    public GetSriErrorLogsForCreditNoteQueryHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<GetSriErrorLogsForCreditNoteQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<List<SriErrorLogDto>>> Handle(
        GetSriErrorLogsForCreditNoteQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId
            ?? throw new UnauthorizedAccessException("Tenant context is not set");

        var creditNote = await _unitOfWork.CreditNotes.GetByIdAsync(request.CreditNoteId, cancellationToken);
        if (creditNote == null || creditNote.TenantId != tenantId)
        {
            _logger.LogWarning("Credit note {CreditNoteId} not found for tenant {TenantId}", request.CreditNoteId, tenantId);
            return Result<List<SriErrorLogDto>>.Failure("Credit note not found");
        }

        var logs = await _unitOfWork.SriErrorLogs.GetByCreditNoteIdAsync(request.CreditNoteId, tenantId, cancellationToken);

        var dtos = logs
            .OrderByDescending(l => l.OccurredAt)
            .Select(l => new SriErrorLogDto
            {
                Id = l.Id,
                Operation = l.Operation,
                ErrorCode = l.ErrorCode,
                ErrorMessage = l.ErrorMessage,
                AdditionalData = l.AdditionalData,
                OccurredAt = l.OccurredAt
            })
            .ToList();

        return Result<List<SriErrorLogDto>>.Success(dtos);
    }
}
