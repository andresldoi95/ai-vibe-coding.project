using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Payments.Queries.GetPaymentsByInvoiceId;

/// <summary>
/// Query to get all payments for a specific invoice
/// </summary>
public record GetPaymentsByInvoiceIdQuery(Guid InvoiceId) : IRequest<Result<List<PaymentDto>>>;
