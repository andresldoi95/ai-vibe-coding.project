using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Invoices.Queries.GetSriErrorLogs;

public record GetSriErrorLogsForInvoiceQuery(Guid InvoiceId) : IRequest<Result<List<SriErrorLogDto>>>;
