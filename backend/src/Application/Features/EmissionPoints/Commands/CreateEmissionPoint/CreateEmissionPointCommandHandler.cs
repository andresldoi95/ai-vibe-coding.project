using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;
using SaaS.Domain.Entities;

namespace SaaS.Application.Features.EmissionPoints.Commands.CreateEmissionPoint;

/// <summary>
/// Handler for creating a new emission point
/// </summary>
public class CreateEmissionPointCommandHandler : IRequestHandler<CreateEmissionPointCommand, Result<EmissionPointDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<CreateEmissionPointCommandHandler> _logger;

    public CreateEmissionPointCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<CreateEmissionPointCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<EmissionPointDto>> Handle(CreateEmissionPointCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_tenantContext.TenantId.HasValue)
            {
                return Result<EmissionPointDto>.Failure("Tenant context is required");
            }

            // Verify establishment exists and belongs to tenant
            var establishment = await _unitOfWork.Establishments.GetByIdAsync(request.EstablishmentId, cancellationToken);
            if (establishment == null || establishment.TenantId != _tenantContext.TenantId.Value)
            {
                return Result<EmissionPointDto>.Failure("Establishment not found");
            }

            // Create emission point entity
            var emissionPoint = new EmissionPoint
            {
                TenantId = _tenantContext.TenantId.Value,
                EstablishmentId = request.EstablishmentId,
                EmissionPointCode = request.EmissionPointCode,
                Name = request.Name,
                IsActive = request.IsActive,
                InvoiceSequence = 1,
                CreditNoteSequence = 1,
                DebitNoteSequence = 1,
                RetentionSequence = 1
            };

            await _unitOfWork.EmissionPoints.AddAsync(emissionPoint, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Emission point {Code} created successfully for establishment {EstablishmentId}",
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
            _logger.LogError(ex, "Error creating emission point for tenant {TenantId}", _tenantContext.TenantId);
            return Result<EmissionPointDto>.Failure($"Failed to create emission point: {ex.Message}");
        }
    }
}
