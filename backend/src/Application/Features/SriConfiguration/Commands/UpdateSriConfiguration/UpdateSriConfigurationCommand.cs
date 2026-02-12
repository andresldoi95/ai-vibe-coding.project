using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;
using SaaS.Domain.Enums;

namespace SaaS.Application.Features.SriConfiguration.Commands.UpdateSriConfiguration;

/// <summary>
/// Command to update SRI configuration
/// </summary>
public record UpdateSriConfigurationCommand : IRequest<Result<SriConfigurationDto>>
{
    public string CompanyRuc { get; init; } = string.Empty;
    public string LegalName { get; init; } = string.Empty;
    public string? TradeName { get; init; }
    public string MainAddress { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public SriEnvironment Environment { get; init; }
    public bool AccountingRequired { get; init; }
    public string? SpecialTaxpayerNumber { get; init; }
    public bool IsRiseRegime { get; init; }
}
