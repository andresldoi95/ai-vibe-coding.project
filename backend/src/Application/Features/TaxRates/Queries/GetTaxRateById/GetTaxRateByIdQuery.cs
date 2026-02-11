using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.TaxRates.Queries.GetTaxRateById;

public record GetTaxRateByIdQuery(Guid Id) : IRequest<Result<TaxRateDto>>;
