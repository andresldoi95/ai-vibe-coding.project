using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.TaxRates.Queries.GetAllTaxRates;

public class GetAllTaxRatesQueryHandler : IRequestHandler<GetAllTaxRatesQuery, Result<List<TaxRateDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetAllTaxRatesQueryHandler> _logger;

    public GetAllTaxRatesQueryHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<GetAllTaxRatesQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<List<TaxRateDto>>> Handle(GetAllTaxRatesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var tenantId = _tenantContext.TenantId ?? throw new UnauthorizedAccessException("Tenant context is not set");

            var taxRates = await _unitOfWork.TaxRates.GetActiveByTenantAsync(tenantId, cancellationToken);

            var taxRateDtos = taxRates.Select(tr => new TaxRateDto
            {
                Id = tr.Id,
                TenantId = tr.TenantId,
                Code = tr.Code,
                Name = tr.Name,
                Rate = tr.Rate,
                IsDefault = tr.IsDefault,
                IsActive = tr.IsActive,
                Description = tr.Description,
                Country = tr.Country,
                CreatedAt = tr.CreatedAt,
                UpdatedAt = tr.UpdatedAt
            }).ToList();

            return Result<List<TaxRateDto>>.Success(taxRateDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tax rates");
            return Result<List<TaxRateDto>>.Failure("An error occurred while retrieving tax rates");
        }
    }
}
