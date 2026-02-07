using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Warehouses.Queries.GetWarehouseById;

/// <summary>
/// Handler for retrieving a warehouse by ID
/// </summary>
public class GetWarehouseByIdQueryHandler : IRequestHandler<GetWarehouseByIdQuery, Result<WarehouseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetWarehouseByIdQueryHandler> _logger;

    public GetWarehouseByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<GetWarehouseByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<WarehouseDto>> Handle(GetWarehouseByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_tenantContext.TenantId.HasValue)
            {
                return Result<WarehouseDto>.Failure("Tenant context is required");
            }

            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(request.Id, cancellationToken);

            if (warehouse == null || warehouse.IsDeleted)
            {
                return Result<WarehouseDto>.Failure($"Warehouse with ID '{request.Id}' not found");
            }

            // Verify warehouse belongs to current tenant
            if (warehouse.TenantId != _tenantContext.TenantId.Value)
            {
                return Result<WarehouseDto>.Failure("Access denied");
            }

            var warehouseDto = new WarehouseDto
            {
                Id = warehouse.Id,
                TenantId = warehouse.TenantId,
                Name = warehouse.Name,
                Code = warehouse.Code,
                Description = warehouse.Description,
                StreetAddress = warehouse.StreetAddress,
                City = warehouse.City,
                State = warehouse.State,
                PostalCode = warehouse.PostalCode,
                Country = warehouse.Country,
                Phone = warehouse.Phone,
                Email = warehouse.Email,
                IsActive = warehouse.IsActive,
                SquareFootage = warehouse.SquareFootage,
                Capacity = warehouse.Capacity,
                CreatedAt = warehouse.CreatedAt,
                UpdatedAt = warehouse.UpdatedAt
            };

            _logger.LogInformation(
                "Retrieved warehouse {Id} for tenant {TenantId}",
                warehouse.Id,
                _tenantContext.TenantId.Value);

            return Result<WarehouseDto>.Success(warehouseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving warehouse {Id}", request.Id);
            return Result<WarehouseDto>.Failure("An error occurred while retrieving the warehouse");
        }
    }
}
