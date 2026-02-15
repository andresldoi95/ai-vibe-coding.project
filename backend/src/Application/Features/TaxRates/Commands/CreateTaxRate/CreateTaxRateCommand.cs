using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.TaxRates.Commands.CreateTaxRate;

public record CreateTaxRateCommand : IRequest<Result<TaxRateDto>>
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public decimal Rate { get; init; }
    public bool IsDefault { get; init; }
    public bool IsActive { get; init; } = true;
    public string? Description { get; init; }
    public Guid? CountryId { get; init; }
}
