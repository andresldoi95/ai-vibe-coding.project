using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;

namespace SaaS.Application.Features.Payments.Commands.CreatePayment;

/// <summary>
/// Handler for creating a new payment
/// </summary>
public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, Result<PaymentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<CreatePaymentCommandHandler> _logger;

    public CreatePaymentCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<CreatePaymentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<PaymentDto>> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_tenantContext.TenantId.HasValue)
            {
                return Result<PaymentDto>.Failure("Tenant context is required");
            }

            // Verify invoice exists and belongs to tenant
            var invoice = await _unitOfWork.Invoices.GetByIdAsync(
                request.InvoiceId,
                cancellationToken);

            if (invoice == null || invoice.IsDeleted)
            {
                return Result<PaymentDto>.Failure("Invoice not found");
            }

            // Verify invoice belongs to current tenant
            if (invoice.TenantId != _tenantContext.TenantId.Value)
            {
                return Result<PaymentDto>.Failure("Access denied");
            }

            // Get existing completed payments for this invoice
            var existingPayments = await _unitOfWork.Payments.GetByInvoiceIdAsync(
                request.InvoiceId,
                _tenantContext.TenantId.Value,
                cancellationToken);

            var totalPaid = existingPayments
                .Where(p => p.Status == PaymentStatus.Completed)
                .Sum(p => p.Amount);

            var remainingBalance = invoice.TotalAmount - totalPaid;

            // Validate amount doesn't exceed remaining balance
            if (request.Amount > remainingBalance)
            {
                return Result<PaymentDto>.Failure($"Payment amount cannot exceed remaining balance of {remainingBalance:C}");
            }

            // Create payment entity
            var payment = new Payment
            {
                TenantId = _tenantContext.TenantId.Value,
                InvoiceId = request.InvoiceId,
                Amount = request.Amount,
                PaymentDate = DateTime.SpecifyKind(request.PaymentDate, DateTimeKind.Utc),
                PaymentMethod = request.PaymentMethod,
                Status = request.Status,
                TransactionId = request.TransactionId,
                Notes = request.Notes
            };

            await _unitOfWork.Payments.AddAsync(payment, cancellationToken);

            // Update invoice status if payment is completed
            if (request.Status == PaymentStatus.Completed)
            {
                var newTotalPaid = totalPaid + request.Amount;

                if (newTotalPaid >= invoice.TotalAmount)
                {
                    invoice.Status = InvoiceStatus.Paid;
                    invoice.PaidDate = DateTime.SpecifyKind(request.PaymentDate, DateTimeKind.Utc);

                    _logger.LogInformation(
                        "Invoice {InvoiceNumber} marked as Paid after payment of {Amount}",
                        invoice.InvoiceNumber,
                        request.Amount);
                }

                // Update the invoice
                await _unitOfWork.Invoices.UpdateAsync(invoice, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Payment of {Amount} created for Invoice {InvoiceNumber} with status {Status}",
                payment.Amount,
                invoice.InvoiceNumber,
                payment.Status);

            // Load related data for DTO
            var createdPayment = await _unitOfWork.Payments.GetByIdAsync(
                payment.Id,
                cancellationToken);

            if (createdPayment == null)
            {
                return Result<PaymentDto>.Failure("Failed to retrieve created payment");
            }

            // Map to DTO
            var paymentDto = new PaymentDto
            {
                Id = createdPayment.Id,
                TenantId = createdPayment.TenantId,
                InvoiceId = createdPayment.InvoiceId,
                InvoiceNumber = invoice.InvoiceNumber,
                CustomerName = invoice.Customer?.Name ?? string.Empty,
                Amount = createdPayment.Amount,
                PaymentDate = createdPayment.PaymentDate,
                PaymentMethod = createdPayment.PaymentMethod,
                Status = createdPayment.Status,
                TransactionId = createdPayment.TransactionId,
                Notes = createdPayment.Notes,
                CreatedAt = createdPayment.CreatedAt,
                UpdatedAt = createdPayment.UpdatedAt
            };

            return Result<PaymentDto>.Success(paymentDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment for invoice {InvoiceId}", request.InvoiceId);
            return Result<PaymentDto>.Failure($"Failed to create payment: {ex.Message}");
        }
    }
}
