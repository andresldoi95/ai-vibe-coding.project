using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Establishments.Queries.GetAllEstablishments;

/// <summary>
/// Handler for retrieving all establishments
/// </summary>
public class GetAllEstablishmentsQueryHandler : IRequestHandler<GetAllEstablishmentsQuery, Result<List<EstablishmentDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetAllEstablishmentsQueryHandler> _logger;

    public GetAllEstablishmentsQueryHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<GetAllEstablishmentsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<List<EstablishmentDto>>> Handle(GetAllEstablishmentsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_tenantContext.TenantId.HasValue)
            {
                return Result<List<EstablishmentDto>>.Failure("Tenant context is required");
            }

            var establishments = await _unitOfWork.Establishments.GetAllAsync(cancellationToken);

            var establishmentDtos = establishments.Select(e => new EstablishmentDto
            {
                Id = e.Id,
                TenantId = e.TenantId,
                EstablishmentCode = e.EstablishmentCode,
                Name = e.Name,
                Address = e.Address,
                Phone = e.Phone,
                IsActive = e.IsActive,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt
            }).ToList();

            return Result<List<EstablishmentDto>>.Success(establishmentDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving establishments for tenant {TenantId}", _tenantContext.TenantId);
            return Result<List<EstablishmentDto>>.Failure($"Failed to retrieve establishments: {ex.Message}");
        }
    }
}
