using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.EmissionPoints.Queries.GetEmissionPointById;

/// <summary>
/// Handler for retrieving an emission point by ID
/// </summary>
public class GetEmissionPointByIdQueryHandler : IRequestHandler<GetEmissionPointByIdQuery, Result<EmissionPointDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetEmissionPointByIdQueryHandler> _logger;

    public GetEmissionPointByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<GetEmissionPointByIdQueryHandler> _logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        this._logger = _logger;
    }

    public async Task<Result<EmissionPointDto>> Handle(GetEmissionPointByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_tenantContext.TenantId.HasValue)
            {
                return Result<EmissionPointDto>.Failure("Tenant context is required");
            }

            var emissionPoint = await _unitOfWork.EmissionPoints.GetByIdAsync(request.Id, cancellationToken);

            if (emissionPoint == null || emissionPoint.TenantId != _tenantContext.TenantId.Value)
            {
                return Result<EmissionPointDto>.Failure("Emission point not found");
            }

            var emissionPointDto = new EmissionPointDto
            {
                Id = emissionPoint.Id,
                EstablishmentId = emissionPoint.EstablishmentId,
                EstablishmentCode = emissionPoint.Establishment?.EstablishmentCode,
                EstablishmentName = emissionPoint.Establishment?.Name,
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
            _logger.LogError(ex, "Error retrieving emission point {Id} for tenant {TenantId}", request.Id, _tenantContext.TenantId);
            return Result<EmissionPointDto>.Failure($"Failed to retrieve emission point: {ex.Message}");
        }
    }
}
