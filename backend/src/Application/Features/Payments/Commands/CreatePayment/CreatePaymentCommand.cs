using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;
using SaaS.Domain.Enums;

namespace SaaS.Application.Features.Payments.Commands.CreatePayment;

/// <summary>
/// Command to create a new payment
/// </summary>
public record CreatePaymentCommand : IRequest<Result<PaymentDto>>
{
    public Guid InvoiceId { get; init; }
    public decimal Amount { get; init; }
    public DateTime PaymentDate { get; init; }
    public SriPaymentMethod PaymentMethod { get; init; }
    public PaymentStatus Status { get; init; } = PaymentStatus.Pending;
    public string? TransactionId { get; init; }
    public string? Notes { get; init; }
}
