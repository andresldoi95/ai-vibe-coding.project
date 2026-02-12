using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;
using SaaS.Domain.Entities;

namespace SaaS.Application.Features.TaxRates.Commands.CreateTaxRate;

public class CreateTaxRateCommandHandler : IRequestHandler<CreateTaxRateCommand, Result<TaxRateDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<CreateTaxRateCommandHandler> _logger;

    public CreateTaxRateCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<CreateTaxRateCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<TaxRateDto>> Handle(CreateTaxRateCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tenantId = _tenantContext.TenantId ?? throw new UnauthorizedAccessException("Tenant context is not set");

            // Check if code already exists for this tenant
            var existingTaxRate = await _unitOfWork.TaxRates.GetByCodeAsync(request.Code, tenantId, cancellationToken);
            if (existingTaxRate != null)
            {
                return Result<TaxRateDto>.Failure($"A tax rate with code '{request.Code}' already exists");
            }

            var taxRate = new TaxRate
            {
                TenantId = tenantId,
                Code = request.Code,
                Name = request.Name,
                Rate = request.Rate,
                IsDefault = request.IsDefault,
                IsActive = request.IsActive,
                Description = request.Description,
                CountryId = request.CountryId
            };

            await _unitOfWork.TaxRates.AddAsync(taxRate, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Tax rate {TaxRateCode} created successfully for tenant {TenantId}", request.Code, tenantId);

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
                CountryCode = null,
                CountryName = null,
                CreatedAt = taxRate.CreatedAt,
                UpdatedAt = taxRate.UpdatedAt
            };

            return Result<TaxRateDto>.Success(taxRateDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tax rate");
            return Result<TaxRateDto>.Failure("An error occurred while creating the tax rate");
        }
    }
}
