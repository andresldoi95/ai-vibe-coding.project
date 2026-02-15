using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;
using SaaS.Domain.Enums;

namespace SaaS.Application.Features.Payments.Commands.VoidPayment;

/// <summary>
/// Handler for voiding a payment
/// </summary>
public class VoidPaymentCommandHandler : IRequestHandler<VoidPaymentCommand, Result<PaymentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<VoidPaymentCommandHandler> _logger;

    public VoidPaymentCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<VoidPaymentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<PaymentDto>> Handle(VoidPaymentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_tenantContext.TenantId.HasValue)
            {
                return Result<PaymentDto>.Failure("Tenant context is required");
            }

            // Get payment
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

            // Validate payment can be voided
            if (payment.Status == PaymentStatus.Voided)
            {
                return Result<PaymentDto>.Failure("Payment is already voided");
            }

            var wasCompleted = payment.Status == PaymentStatus.Completed;

            // Void the payment
            payment.Status = PaymentStatus.Voided;

            // Append void reason to notes
            if (!string.IsNullOrEmpty(request.Reason))
            {
                payment.Notes = string.IsNullOrEmpty(payment.Notes)
                    ? $"Voided: {request.Reason}"
                    : $"{payment.Notes}\nVoided: {request.Reason}";
            }

            // If payment was completed, recalculate invoice status
            if (wasCompleted)
            {
                var paymentInvoice = await _unitOfWork.Invoices.GetByIdAsync(
                    payment.InvoiceId,
                    cancellationToken);

                if (paymentInvoice != null)
                {
                    // Get all completed payments (excluding this voided one)
                    var completedPayments = await _unitOfWork.Payments.GetByInvoiceIdAsync(
                        payment.InvoiceId,
                        _tenantContext.TenantId.Value,
                        cancellationToken);

                    var totalPaid = completedPayments
                        .Where(p => p.Status == PaymentStatus.Completed && p.Id != payment.Id)
                        .Sum(p => p.Amount);

                    // Update invoice status
                    if (totalPaid < paymentInvoice.TotalAmount && paymentInvoice.Status == InvoiceStatus.Paid)
                    {
                        // Determine appropriate status - default to Sent if it was previously sent
                        paymentInvoice.Status = InvoiceStatus.Sent;
                        paymentInvoice.PaidDate = null;

                        _logger.LogInformation(
                            "Invoice {InvoiceNumber} status changed from Paid to {NewStatus} after payment void",
                            paymentInvoice.InvoiceNumber,
                            paymentInvoice.Status);
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Payment {PaymentId} voided for Invoice {InvoiceId}",
                payment.Id,
                payment.InvoiceId);

            // Load related data for DTO
            var voidedPayment = await _unitOfWork.Payments.GetByIdAsync(
                payment.Id,
                cancellationToken);

            if (voidedPayment == null)
            {
                return Result<PaymentDto>.Failure("Failed to retrieve voided payment");
            }

            var invoice = await _unitOfWork.Invoices.GetByIdAsync(
                voidedPayment.InvoiceId,
                cancellationToken);

            // Map to DTO
            var paymentDto = new PaymentDto
            {
                Id = voidedPayment.Id,
                TenantId = voidedPayment.TenantId,
                InvoiceId = voidedPayment.InvoiceId,
                InvoiceNumber = invoice?.InvoiceNumber ?? string.Empty,
                CustomerName = invoice?.Customer?.Name ?? string.Empty,
                Amount = voidedPayment.Amount,
                PaymentDate = voidedPayment.PaymentDate,
                PaymentMethod = voidedPayment.PaymentMethod,
                Status = voidedPayment.Status,
                TransactionId = voidedPayment.TransactionId,
                Notes = voidedPayment.Notes,
                CreatedAt = voidedPayment.CreatedAt,
                UpdatedAt = voidedPayment.UpdatedAt
            };

            return Result<PaymentDto>.Success(paymentDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error voiding payment {PaymentId}", request.Id);
            return Result<PaymentDto>.Failure($"Failed to void payment: {ex.Message}");
        }
    }
}
