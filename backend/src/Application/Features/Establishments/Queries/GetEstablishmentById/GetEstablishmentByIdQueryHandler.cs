using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Establishments.Queries.GetEstablishmentById;

/// <summary>
/// Handler for retrieving an establishment by ID
/// </summary>
public class GetEstablishmentByIdQueryHandler : IRequestHandler<GetEstablishmentByIdQuery, Result<EstablishmentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetEstablishmentByIdQueryHandler> _logger;

    public GetEstablishmentByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<GetEstablishmentByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<EstablishmentDto>> Handle(GetEstablishmentByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_tenantContext.TenantId.HasValue)
            {
                return Result<EstablishmentDto>.Failure("Tenant context is required");
            }

            var establishment = await _unitOfWork.Establishments.GetByIdAsync(request.Id, cancellationToken);

            if (establishment == null || establishment.TenantId != _tenantContext.TenantId.Value)
            {
                return Result<EstablishmentDto>.Failure("Establishment not found");
            }

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
            _logger.LogError(ex, "Error retrieving establishment {Id} for tenant {TenantId}", request.Id, _tenantContext.TenantId);
            return Result<EstablishmentDto>.Failure($"Failed to retrieve establishment: {ex.Message}");
        }
    }
}
