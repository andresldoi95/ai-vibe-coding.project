using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;
using SaaS.Domain.Enums;

namespace SaaS.Application.Features.Payments.Commands.CompletePayment;

/// <summary>
/// Handler for completing a payment
/// </summary>
public class CompletePaymentCommandHandler : IRequestHandler<CompletePaymentCommand, Result<PaymentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<CompletePaymentCommandHandler> _logger;

    public CompletePaymentCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<CompletePaymentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<PaymentDto>> Handle(CompletePaymentCommand request, CancellationToken cancellationToken)
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

            // Validate payment can be completed
            if (payment.Status == PaymentStatus.Completed)
            {
                return Result<PaymentDto>.Failure("Payment is already completed");
            }

            if (payment.Status == PaymentStatus.Voided)
            {
                return Result<PaymentDto>.Failure("Cannot complete a voided payment");
            }

            // Complete the payment
            payment.Status = PaymentStatus.Completed;

            // Append completion notes if provided
            if (!string.IsNullOrEmpty(request.Notes))
            {
                payment.Notes = string.IsNullOrEmpty(payment.Notes)
                    ? request.Notes
                    : $"{payment.Notes}\n{request.Notes}";
            }

            // Recalculate invoice status
            var paymentInvoice = await _unitOfWork.Invoices.GetByIdAsync(
                payment.InvoiceId,
                cancellationToken);

            if (paymentInvoice != null)
            {
                // Get all completed payments (including this one)
                var completedPayments = await _unitOfWork.Payments.GetByInvoiceIdAsync(
                    payment.InvoiceId,
                    _tenantContext.TenantId.Value,
                    cancellationToken);

                var totalPaid = completedPayments
                    .Where(p => p.Status == PaymentStatus.Completed || p.Id == payment.Id)
                    .Sum(p => p.Amount);

                // Update invoice status to Paid if fully paid
                if (totalPaid >= paymentInvoice.TotalAmount && paymentInvoice.Status != InvoiceStatus.Paid)
                {
                    paymentInvoice.Status = InvoiceStatus.Paid;
                    paymentInvoice.PaidDate = DateTime.UtcNow;

                    _logger.LogInformation(
                        "Invoice {InvoiceNumber} status changed to Paid after payment completion. Total paid: {TotalPaid}, Invoice total: {InvoiceTotal}",
                        paymentInvoice.InvoiceNumber,
                        totalPaid,
                        paymentInvoice.TotalAmount);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Payment {PaymentId} completed for Invoice {InvoiceId}",
                payment.Id,
                payment.InvoiceId);

            // Load related data for DTO
            var completedPayment = await _unitOfWork.Payments.GetByIdAsync(
                payment.Id,
                cancellationToken);

            if (completedPayment == null)
            {
                return Result<PaymentDto>.Failure("Failed to retrieve completed payment");
            }

            var invoice = await _unitOfWork.Invoices.GetByIdAsync(
                completedPayment.InvoiceId,
                cancellationToken);

            // Map to DTO
            var paymentDto = new PaymentDto
            {
                Id = completedPayment.Id,
                TenantId = completedPayment.TenantId,
                InvoiceId = completedPayment.InvoiceId,
                InvoiceNumber = invoice?.InvoiceNumber ?? string.Empty,
                CustomerName = invoice?.Customer?.Name ?? string.Empty,
                Amount = completedPayment.Amount,
                PaymentDate = completedPayment.PaymentDate,
                PaymentMethod = completedPayment.PaymentMethod,
                Status = completedPayment.Status,
                TransactionId = completedPayment.TransactionId,
                Notes = completedPayment.Notes,
                CreatedAt = completedPayment.CreatedAt,
                UpdatedAt = completedPayment.UpdatedAt
            };

            return Result<PaymentDto>.Success(paymentDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing payment {PaymentId}", request.Id);
            return Result<PaymentDto>.Failure($"Failed to complete payment: {ex.Message}");
        }
    }
}
