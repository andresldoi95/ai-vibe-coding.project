using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.TaxRates.Queries.GetTaxRateById;

public class GetTaxRateByIdQueryHandler : IRequestHandler<GetTaxRateByIdQuery, Result<TaxRateDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetTaxRateByIdQueryHandler> _logger;

    public GetTaxRateByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<GetTaxRateByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<TaxRateDto>> Handle(GetTaxRateByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var tenantId = _tenantContext.TenantId ?? throw new UnauthorizedAccessException("Tenant context is not set");

            var taxRate = await _unitOfWork.TaxRates.GetByIdAsync(request.Id, cancellationToken);
            if (taxRate == null || taxRate.TenantId != tenantId)
            {
                return Result<TaxRateDto>.Failure("Tax rate not found");
            }

            var taxRateDto = new TaxRateDto
            {
                Id = taxRate.Id,
                TenantId = taxRate.TenantId,
                Code = taxRate.Code,
                Name = taxRate.Name,
                Rate = taxRate.Rate,
                IsDefault = taxRate.IsDefault,
                IsActive = taxRate.IsActive,
                Description = taxRate.Description,
                CountryId = taxRate.CountryId,
                CountryCode = taxRate.Country?.Code,
                CountryName = taxRate.Country?.Name,
                CreatedAt = taxRate.CreatedAt,
                UpdatedAt = taxRate.UpdatedAt
            };

            return Result<TaxRateDto>.Success(taxRateDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tax rate {TaxRateId}", request.Id);
            return Result<TaxRateDto>.Failure("An error occurred while retrieving the tax rate");
        }
    }
}
