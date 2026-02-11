using MediatR;
using SaaS.Application.Common.Models;

namespace SaaS.Application.Features.TaxRates.Commands.DeleteTaxRate;

public record DeleteTaxRateCommand(Guid Id) : IRequest<Result<bool>>;
