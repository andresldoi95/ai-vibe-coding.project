using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Payments.Queries.GetAllPayments;

/// <summary>
/// Handler for retrieving all payments for the current tenant
/// </summary>
public class GetAllPaymentsQueryHandler : IRequestHandler<GetAllPaymentsQuery, Result<List<PaymentDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetAllPaymentsQueryHandler> _logger;

    public GetAllPaymentsQueryHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<GetAllPaymentsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<List<PaymentDto>>> Handle(GetAllPaymentsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_tenantContext.TenantId.HasValue)
            {
                return Result<List<PaymentDto>>.Failure("Tenant context is required");
            }

            var payments = await _unitOfWork.Payments.GetAllByTenantAsync(
                _tenantContext.TenantId.Value,
                cancellationToken);

            var paymentDtos = payments.Select(p => new PaymentDto
            {
                Id = p.Id,
                TenantId = p.TenantId,
                InvoiceId = p.InvoiceId,
                InvoiceNumber = p.Invoice?.InvoiceNumber ?? string.Empty,
                CustomerName = p.Invoice?.Customer?.Name ?? string.Empty,
                Amount = p.Amount,
                PaymentDate = p.PaymentDate,
                PaymentMethod = p.PaymentMethod,
                Status = p.Status,
                TransactionId = p.TransactionId,
                Notes = p.Notes,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            }).ToList();

            _logger.LogInformation(
                "Retrieved {Count} payments for tenant {TenantId}",
                paymentDtos.Count,
                _tenantContext.TenantId.Value);

            return Result<List<PaymentDto>>.Success(paymentDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payments for tenant {TenantId}", _tenantContext.TenantId);
            return Result<List<PaymentDto>>.Failure("An error occurred while retrieving payments");
        }
    }
}
