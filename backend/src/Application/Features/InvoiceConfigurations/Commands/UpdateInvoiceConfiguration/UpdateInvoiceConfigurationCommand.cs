using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.InvoiceConfigurations.Commands.UpdateInvoiceConfiguration;

public record UpdateInvoiceConfigurationCommand : IRequest<Result<InvoiceConfigurationDto>>
{
    public string EstablishmentCode { get; init; } = string.Empty;
    public string EmissionPointCode { get; init; } = string.Empty;
    public Guid? DefaultTaxRateId { get; init; }
    public Guid? DefaultWarehouseId { get; init; }
    public int DueDays { get; init; }
    public bool RequireCustomerTaxId { get; init; }
}
