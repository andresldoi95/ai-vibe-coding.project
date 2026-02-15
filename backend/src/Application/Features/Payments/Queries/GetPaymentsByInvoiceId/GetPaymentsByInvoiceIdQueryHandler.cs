using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Payments.Queries.GetPaymentsByInvoiceId;

/// <summary>
/// Handler for retrieving all payments for a specific invoice
/// </summary>
public class GetPaymentsByInvoiceIdQueryHandler : IRequestHandler<GetPaymentsByInvoiceIdQuery, Result<List<PaymentDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetPaymentsByInvoiceIdQueryHandler> _logger;

    public GetPaymentsByInvoiceIdQueryHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<GetPaymentsByInvoiceIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<List<PaymentDto>>> Handle(GetPaymentsByInvoiceIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_tenantContext.TenantId.HasValue)
            {
                return Result<List<PaymentDto>>.Failure("Tenant context is required");
            }

            // Verify invoice exists and belongs to tenant
            var invoice = await _unitOfWork.Invoices.GetByIdAsync(
                request.InvoiceId,
                cancellationToken);

            if (invoice == null || invoice.IsDeleted)
            {
                return Result<List<PaymentDto>>.Failure("Invoice not found");
            }

            // Verify invoice belongs to current tenant
            if (invoice.TenantId != _tenantContext.TenantId.Value)
            {
                return Result<List<PaymentDto>>.Failure("Access denied");
            }

            var payments = await _unitOfWork.Payments.GetByInvoiceIdAsync(
                request.InvoiceId,
                _tenantContext.TenantId.Value,
                cancellationToken);

            var paymentDtos = payments.Select(p => new PaymentDto
            {
                Id = p.Id,
                TenantId = p.TenantId,
                InvoiceId = p.InvoiceId,
                InvoiceNumber = invoice.InvoiceNumber,
                CustomerName = invoice.Customer?.Name ?? string.Empty,
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
                "Retrieved {Count} payments for invoice {InvoiceId}",
                paymentDtos.Count,
                request.InvoiceId);

            return Result<List<PaymentDto>>.Success(paymentDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payments for invoice {InvoiceId}", request.InvoiceId);
            return Result<List<PaymentDto>>.Failure("An error occurred while retrieving invoice payments");
        }
    }
}
