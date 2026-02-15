using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Payments.Commands.VoidPayment;

/// <summary>
/// Command to void an existing payment
/// </summary>
public record VoidPaymentCommand : IRequest<Result<PaymentDto>>
{
    public Guid Id { get; init; }
    public string? Reason { get; init; }
}
