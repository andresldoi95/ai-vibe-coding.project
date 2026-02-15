using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Payments.Commands.CompletePayment;

/// <summary>
/// Command to complete an existing payment
/// </summary>
public record CompletePaymentCommand : IRequest<Result<PaymentDto>>
{
    public Guid Id { get; init; }
    public string? Notes { get; init; }
}
