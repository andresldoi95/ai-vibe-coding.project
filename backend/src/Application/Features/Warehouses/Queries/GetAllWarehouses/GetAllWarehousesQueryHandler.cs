using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Warehouses.Queries.GetAllWarehouses;

/// <summary>
/// Handler for retrieving all warehouses for the current tenant
/// </summary>
public class GetAllWarehousesQueryHandler : IRequestHandler<GetAllWarehousesQuery, Result<List<WarehouseDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetAllWarehousesQueryHandler> _logger;

    public GetAllWarehousesQueryHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<GetAllWarehousesQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<List<WarehouseDto>>> Handle(GetAllWarehousesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_tenantContext.TenantId.HasValue)
            {
                return Result<List<WarehouseDto>>.Failure("Tenant context is required");
            }

            var warehouses = await _unitOfWork.Warehouses.GetAllByTenantAsync(
                _tenantContext.TenantId.Value,
                cancellationToken);

            var warehouseDtos = warehouses.Select(w => new WarehouseDto
            {
                Id = w.Id,
                TenantId = w.TenantId,
                Name = w.Name,
                Code = w.Code,
                Description = w.Description,
                StreetAddress = w.StreetAddress,
                City = w.City,
                State = w.State,
                PostalCode = w.PostalCode,
                Country = w.Country,
                Phone = w.Phone,
                Email = w.Email,
                IsActive = w.IsActive,
                SquareFootage = w.SquareFootage,
                Capacity = w.Capacity,
                CreatedAt = w.CreatedAt,
                UpdatedAt = w.UpdatedAt
            }).ToList();

            _logger.LogInformation(
                "Retrieved {Count} warehouses for tenant {TenantId}",
                warehouseDtos.Count,
                _tenantContext.TenantId.Value);

            return Result<List<WarehouseDto>>.Success(warehouseDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving warehouses for tenant {TenantId}", _tenantContext.TenantId);
            return Result<List<WarehouseDto>>.Failure("An error occurred while retrieving warehouses");
        }
    }
}
