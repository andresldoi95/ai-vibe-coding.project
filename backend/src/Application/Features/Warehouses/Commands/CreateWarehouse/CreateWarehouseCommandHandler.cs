using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;
using SaaS.Domain.Entities;

namespace SaaS.Application.Features.Warehouses.Commands.CreateWarehouse;

/// <summary>
/// Handler for creating a new warehouse
/// </summary>
public class CreateWarehouseCommandHandler : IRequestHandler<CreateWarehouseCommand, Result<WarehouseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<CreateWarehouseCommandHandler> _logger;

    public CreateWarehouseCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<CreateWarehouseCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<WarehouseDto>> Handle(CreateWarehouseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_tenantContext.TenantId.HasValue)
            {
                return Result<WarehouseDto>.Failure("Tenant context is required");
            }

            // Check if warehouse code already exists for this tenant
            var existingWarehouse = await _unitOfWork.Warehouses.GetByCodeAsync(
                request.Code,
                _tenantContext.TenantId.Value,
                cancellationToken);

            if (existingWarehouse != null)
            {
                return Result<WarehouseDto>.Failure($"Warehouse with code '{request.Code}' already exists");
            }

            // Create warehouse entity
            var warehouse = new Warehouse
            {
                TenantId = _tenantContext.TenantId.Value,
                Name = request.Name,
                Code = request.Code,
                Description = request.Description,
                StreetAddress = request.StreetAddress,
                City = request.City,
                State = request.State,
                PostalCode = request.PostalCode,
                CountryId = request.CountryId,
                Phone = request.Phone,
                Email = request.Email,
                IsActive = request.IsActive,
                SquareFootage = request.SquareFootage,
                Capacity = request.Capacity
            };

            await _unitOfWork.Warehouses.AddAsync(warehouse, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Load Country for DTO mapping
            var country = await _unitOfWork.Countries.GetByIdAsync(warehouse.CountryId);

            _logger.LogInformation(
                "Warehouse {Code} created successfully for tenant {TenantId}",
                warehouse.Code,
                warehouse.TenantId);

            // Map to DTO
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
                CountryId = warehouse.CountryId,
                CountryName = country?.Name ?? string.Empty,
                CountryCode = country?.Code ?? string.Empty,
                Phone = warehouse.Phone,
                Email = warehouse.Email,
                IsActive = warehouse.IsActive,
                SquareFootage = warehouse.SquareFootage,
                Capacity = warehouse.Capacity,
                CreatedAt = warehouse.CreatedAt,
                UpdatedAt = warehouse.UpdatedAt
            };

            return Result<WarehouseDto>.Success(warehouseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating warehouse");
            return Result<WarehouseDto>.Failure("An error occurred while creating the warehouse");
        }
    }
}
