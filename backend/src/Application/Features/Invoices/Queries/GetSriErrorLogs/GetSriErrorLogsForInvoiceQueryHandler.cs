using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Invoices.Queries.GetSriErrorLogs;

public class GetSriErrorLogsForInvoiceQueryHandler
    : IRequestHandler<GetSriErrorLogsForInvoiceQuery, Result<List<SriErrorLogDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetSriErrorLogsForInvoiceQueryHandler> _logger;

    public GetSriErrorLogsForInvoiceQueryHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<GetSriErrorLogsForInvoiceQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<List<SriErrorLogDto>>> Handle(
        GetSriErrorLogsForInvoiceQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId
            ?? throw new UnauthorizedAccessException("Tenant context is not set");

        // Verify invoice belongs to this tenant
        var invoice = await _unitOfWork.Invoices.GetByIdAsync(request.InvoiceId, cancellationToken);
        if (invoice == null || invoice.TenantId != tenantId)
        {
            _logger.LogWarning("Invoice {InvoiceId} not found for tenant {TenantId}", request.InvoiceId, tenantId);
            return Result<List<SriErrorLogDto>>.Failure("Invoice not found");
        }

        var logs = await _unitOfWork.SriErrorLogs.GetByInvoiceIdAsync(request.InvoiceId, tenantId, cancellationToken);

        var dtos = logs
            .OrderByDescending(l => l.OccurredAt)
            .Select(l => new SriErrorLogDto
            {
                Id = l.Id,
                Operation = l.Operation,
                ErrorCode = l.ErrorCode,
                ErrorMessage = l.ErrorMessage,
                AdditionalData = l.AdditionalData,
                OccurredAt = l.OccurredAt,
            })
            .ToList();

        return Result<List<SriErrorLogDto>>.Success(dtos);
    }
}
