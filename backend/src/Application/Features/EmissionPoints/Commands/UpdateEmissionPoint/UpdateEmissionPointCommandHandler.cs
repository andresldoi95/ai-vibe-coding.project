using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.EmissionPoints.Commands.UpdateEmissionPoint;

/// <summary>
/// Handler for updating an existing emission point
/// </summary>
public class UpdateEmissionPointCommandHandler : IRequestHandler<UpdateEmissionPointCommand, Result<EmissionPointDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<UpdateEmissionPointCommandHandler> _logger;

    public UpdateEmissionPointCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<UpdateEmissionPointCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<EmissionPointDto>> Handle(UpdateEmissionPointCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_tenantContext.TenantId.HasValue)
            {
                return Result<EmissionPointDto>.Failure("Tenant context is required");
            }

            var emissionPoint = await _unitOfWork.EmissionPoints.GetByIdAsync(request.Id, cancellationToken);

            if (emissionPoint == null)
            {
                return Result<EmissionPointDto>.Failure("Emission point not found");
            }

            if (emissionPoint.TenantId != _tenantContext.TenantId.Value)
            {
                return Result<EmissionPointDto>.Failure("Emission point not found");
            }

            // Verify establishment exists and belongs to tenant
            var establishment = await _unitOfWork.Establishments.GetByIdAsync(request.EstablishmentId, cancellationToken);
            if (establishment == null || establishment.TenantId != _tenantContext.TenantId.Value)
            {
                return Result<EmissionPointDto>.Failure("Establishment not found");
            }

            // Update emission point
            emissionPoint.EstablishmentId = request.EstablishmentId;
            emissionPoint.EmissionPointCode = request.EmissionPointCode;
            emissionPoint.Name = request.Name;
            emissionPoint.IsActive = request.IsActive;

            await _unitOfWork.EmissionPoints.UpdateAsync(emissionPoint, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Emission point {Code} updated successfully for establishment {EstablishmentId}",
                emissionPoint.EmissionPointCode,
                emissionPoint.EstablishmentId);

            // Map to DTO
            var emissionPointDto = new EmissionPointDto
            {
                Id = emissionPoint.Id,
                EstablishmentId = emissionPoint.EstablishmentId,
                EstablishmentCode = establishment.EstablishmentCode,
                EstablishmentName = establishment.Name,
                EmissionPointCode = emissionPoint.EmissionPointCode,
                Name = emissionPoint.Name,
                IsActive = emissionPoint.IsActive,
                InvoiceSequence = emissionPoint.InvoiceSequence,
                CreditNoteSequence = emissionPoint.CreditNoteSequence,
                DebitNoteSequence = emissionPoint.DebitNoteSequence,
                RetentionSequence = emissionPoint.RetentionSequence,
                CreatedAt = emissionPoint.CreatedAt,
                UpdatedAt = emissionPoint.UpdatedAt
            };

            return Result<EmissionPointDto>.Success(emissionPointDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating emission point {Id} for tenant {TenantId}", request.Id, _tenantContext.TenantId);
            return Result<EmissionPointDto>.Failure($"Failed to update emission point: {ex.Message}");
        }
    }
}
