using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Payments.Queries.GetPaymentById;

/// <summary>
/// Query to get a payment by ID
/// </summary>
public record GetPaymentByIdQuery(Guid Id) : IRequest<Result<PaymentDto>>;
