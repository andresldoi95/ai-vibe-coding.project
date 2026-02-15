using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Payments.Queries.GetPaymentById;

/// <summary>
/// Handler for retrieving a payment by ID
/// </summary>
public class GetPaymentByIdQueryHandler : IRequestHandler<GetPaymentByIdQuery, Result<PaymentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetPaymentByIdQueryHandler> _logger;

    public GetPaymentByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<GetPaymentByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<PaymentDto>> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_tenantContext.TenantId.HasValue)
            {
                return Result<PaymentDto>.Failure("Tenant context is required");
            }

            var payment = await _unitOfWork.Payments.GetByIdAsync(
                request.Id,
                cancellationToken);

            if (payment == null || payment.IsDeleted)
            {
                return Result<PaymentDto>.Failure("Payment not found");
            }

            // Verify payment belongs to current tenant
            if (payment.TenantId != _tenantContext.TenantId.Value)
            {
                return Result<PaymentDto>.Failure("Access denied");
            }

            // Load invoice and customer for DTO
            var invoice = await _unitOfWork.Invoices.GetByIdAsync(
                payment.InvoiceId,
                cancellationToken);

            var paymentDto = new PaymentDto
            {
                Id = payment.Id,
                TenantId = payment.TenantId,
                InvoiceId = payment.InvoiceId,
                InvoiceNumber = invoice?.InvoiceNumber ?? string.Empty,
                CustomerName = invoice?.Customer?.Name ?? string.Empty,
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate,
                PaymentMethod = payment.PaymentMethod,
                Status = payment.Status,
                TransactionId = payment.TransactionId,
                Notes = payment.Notes,
                CreatedAt = payment.CreatedAt,
                UpdatedAt = payment.UpdatedAt
            };

            return Result<PaymentDto>.Success(paymentDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment {PaymentId}", request.Id);
            return Result<PaymentDto>.Failure("An error occurred while retrieving the payment");
        }
    }
}
