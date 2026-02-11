using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.InvoiceConfigurations.Queries.GetInvoiceConfiguration;

public record GetInvoiceConfigurationQuery : IRequest<Result<InvoiceConfigurationDto>>;
