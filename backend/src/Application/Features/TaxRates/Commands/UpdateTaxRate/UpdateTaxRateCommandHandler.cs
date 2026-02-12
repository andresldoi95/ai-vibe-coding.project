using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.TaxRates.Commands.UpdateTaxRate;

public class UpdateTaxRateCommandHandler : IRequestHandler<UpdateTaxRateCommand, Result<TaxRateDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<UpdateTaxRateCommandHandler> _logger;

    public UpdateTaxRateCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<UpdateTaxRateCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<TaxRateDto>> Handle(UpdateTaxRateCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tenantId = _tenantContext.TenantId ?? throw new UnauthorizedAccessException("Tenant context is not set");

            var taxRate = await _unitOfWork.TaxRates.GetByIdAsync(request.Id, cancellationToken);
            if (taxRate == null || taxRate.TenantId != tenantId)
            {
                return Result<TaxRateDto>.Failure("Tax rate not found");
            }

            // Check if code already exists for another tax rate
            var existingTaxRate = await _unitOfWork.TaxRates.GetByCodeAsync(request.Code, tenantId, cancellationToken);
            if (existingTaxRate != null && existingTaxRate.Id != request.Id)
            {
                return Result<TaxRateDto>.Failure($"A tax rate with code '{request.Code}' already exists");
            }

            taxRate.Code = request.Code;
            taxRate.Name = request.Name;
            taxRate.Rate = request.Rate;
            taxRate.IsDefault = request.IsDefault;
            taxRate.IsActive = request.IsActive;
            taxRate.Description = request.Description;
            taxRate.CountryId = request.CountryId;

            await _unitOfWork.TaxRates.UpdateAsync(taxRate, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Tax rate {TaxRateId} updated successfully for tenant {TenantId}", request.Id, tenantId);

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
            _logger.LogError(ex, "Error updating tax rate {TaxRateId}", request.Id);
            return Result<TaxRateDto>.Failure("An error occurred while updating the tax rate");
        }
    }
}
