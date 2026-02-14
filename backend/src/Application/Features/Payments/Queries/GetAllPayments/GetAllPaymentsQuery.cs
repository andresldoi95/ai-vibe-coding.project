using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Payments.Queries.GetAllPayments;

/// <summary>
/// Query to get all payments for the current tenant
/// </summary>
public record GetAllPaymentsQuery : IRequest<Result<List<PaymentDto>>>;
