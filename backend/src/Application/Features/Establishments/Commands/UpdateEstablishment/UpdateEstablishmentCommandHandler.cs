using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Establishments.Commands.UpdateEstablishment;

/// <summary>
/// Handler for updating an existing establishment
/// </summary>
public class UpdateEstablishmentCommandHandler : IRequestHandler<UpdateEstablishmentCommand, Result<EstablishmentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<UpdateEstablishmentCommandHandler> _logger;

    public UpdateEstablishmentCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<UpdateEstablishmentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<EstablishmentDto>> Handle(UpdateEstablishmentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_tenantContext.TenantId.HasValue)
            {
                return Result<EstablishmentDto>.Failure("Tenant context is required");
            }

            var establishment = await _unitOfWork.Establishments.GetByIdAsync(request.Id, cancellationToken);

            if (establishment == null)
            {
                return Result<EstablishmentDto>.Failure("Establishment not found");
            }

            if (establishment.TenantId != _tenantContext.TenantId.Value)
            {
                return Result<EstablishmentDto>.Failure("Establishment not found");
            }

            // Update establishment
            establishment.EstablishmentCode = request.EstablishmentCode;
            establishment.Name = request.Name;
            establishment.Address = request.Address ?? string.Empty;
            establishment.Phone = request.Phone;
            establishment.IsActive = request.IsActive;

            await _unitOfWork.Establishments.UpdateAsync(establishment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Establishment {Code} updated successfully for tenant {TenantId}",
                establishment.EstablishmentCode,
                establishment.TenantId);

            // Map to DTO
            var establishmentDto = new EstablishmentDto
            {
                Id = establishment.Id,
                TenantId = establishment.TenantId,
                EstablishmentCode = establishment.EstablishmentCode,
                Name = establishment.Name,
                Address = establishment.Address,
                Phone = establishment.Phone,
                IsActive = establishment.IsActive,
                CreatedAt = establishment.CreatedAt,
                UpdatedAt = establishment.UpdatedAt
            };

            return Result<EstablishmentDto>.Success(establishmentDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating establishment {Id} for tenant {TenantId}", request.Id, _tenantContext.TenantId);
            return Result<EstablishmentDto>.Failure($"Failed to update establishment: {ex.Message}");
        }
    }
}
