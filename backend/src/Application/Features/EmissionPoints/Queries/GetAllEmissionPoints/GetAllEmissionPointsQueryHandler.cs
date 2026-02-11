using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.EmissionPoints.Queries.GetAllEmissionPoints;

/// <summary>
/// Handler for retrieving all emission points
/// </summary>
public class GetAllEmissionPointsQueryHandler : IRequestHandler<GetAllEmissionPointsQuery, Result<List<EmissionPointDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetAllEmissionPointsQueryHandler> _logger;

    public GetAllEmissionPointsQueryHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<GetAllEmissionPointsQueryHandler> _logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        this._logger = _logger;
    }

    public async Task<Result<List<EmissionPointDto>>> Handle(GetAllEmissionPointsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_tenantContext.TenantId.HasValue)
            {
                return Result<List<EmissionPointDto>>.Failure("Tenant context is required");
            }

            IEnumerable<Domain.Entities.EmissionPoint> emissionPoints;

            if (request.EstablishmentId.HasValue)
            {
                emissionPoints = await _unitOfWork.EmissionPoints.GetByEstablishmentIdAsync(request.EstablishmentId.Value, cancellationToken);
            }
            else
            {
                emissionPoints = await _unitOfWork.EmissionPoints.GetAllAsync(cancellationToken);
            }

            var emissionPointDtos = emissionPoints.Select(e => new EmissionPointDto
            {
                Id = e.Id,
                EstablishmentId = e.EstablishmentId,
                EmissionPointCode = e.EmissionPointCode,
                Name = e.Name,
                IsActive = e.IsActive,
                InvoiceSequence = e.InvoiceSequence,
                CreditNoteSequence = e.CreditNoteSequence,
                DebitNoteSequence = e.DebitNoteSequence,
                RetentionSequence = e.RetentionSequence,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt
            }).ToList();

            return Result<List<EmissionPointDto>>.Success(emissionPointDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving emission points for tenant {TenantId}", _tenantContext.TenantId);
            return Result<List<EmissionPointDto>>.Failure($"Failed to retrieve emission points: {ex.Message}");
        }
    }
}
