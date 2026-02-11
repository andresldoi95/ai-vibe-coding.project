using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.TaxRates.Queries.GetAllTaxRates;

public record GetAllTaxRatesQuery : IRequest<Result<List<TaxRateDto>>>;
