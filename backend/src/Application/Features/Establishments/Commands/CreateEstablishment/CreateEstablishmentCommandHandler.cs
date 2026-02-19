using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;
using SaaS.Domain.Entities;

namespace SaaS.Application.Features.Establishments.Commands.CreateEstablishment;

/// <summary>
/// Handler for creating a new establishment
/// </summary>
public class CreateEstablishmentCommandHandler : IRequestHandler<CreateEstablishmentCommand, Result<EstablishmentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<CreateEstablishmentCommandHandler> _logger;

    public CreateEstablishmentCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<CreateEstablishmentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<EstablishmentDto>> Handle(CreateEstablishmentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_tenantContext.TenantId.HasValue)
            {
                return Result<EstablishmentDto>.Failure("Tenant context is required");
            }

            // Create establishment entity
            var establishment = new Establishment
            {
                TenantId = _tenantContext.TenantId.Value,
                EstablishmentCode = request.EstablishmentCode,
                Name = request.Name,
                Address = request.Address ?? string.Empty,
                Phone = request.Phone,
                IsActive = request.IsActive
            };

            await _unitOfWork.Establishments.AddAsync(establishment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Establishment {Code} created successfully for tenant {TenantId}",
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
            _logger.LogError(ex, "Error creating establishment for tenant {TenantId}", _tenantContext.TenantId);
            return Result<EstablishmentDto>.Failure($"Failed to create establishment: {ex.Message}");
        }
    }
}
