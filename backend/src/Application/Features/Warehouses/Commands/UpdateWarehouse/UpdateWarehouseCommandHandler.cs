using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Warehouses.Commands.UpdateWarehouse;

/// <summary>
/// Handler for updating an existing warehouse
/// </summary>
public class UpdateWarehouseCommandHandler : IRequestHandler<UpdateWarehouseCommand, Result<WarehouseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<UpdateWarehouseCommandHandler> _logger;

    public UpdateWarehouseCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<UpdateWarehouseCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<WarehouseDto>> Handle(UpdateWarehouseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_tenantContext.TenantId.HasValue)
            {
                return Result<WarehouseDto>.Failure("Tenant context is required");
            }

            // Get existing warehouse
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

            // Check if code is being changed and if new code already exists
            if (warehouse.Code != request.Code)
            {
                var existingWarehouse = await _unitOfWork.Warehouses.GetByCodeAsync(
                    request.Code,
                    _tenantContext.TenantId.Value,
                    cancellationToken);

                if (existingWarehouse != null)
                {
                    return Result<WarehouseDto>.Failure($"Warehouse with code '{request.Code}' already exists");
                }
            }

            // Update warehouse properties
            warehouse.Name = request.Name;
            warehouse.Code = request.Code;
            warehouse.Description = request.Description;
            warehouse.StreetAddress = request.StreetAddress;
            warehouse.City = request.City;
            warehouse.State = request.State;
            warehouse.PostalCode = request.PostalCode;
            warehouse.Country = request.Country;
            warehouse.Phone = request.Phone;
            warehouse.Email = request.Email;
            warehouse.IsActive = request.IsActive;
            warehouse.SquareFootage = request.SquareFootage;
            warehouse.Capacity = request.Capacity;

            await _unitOfWork.Warehouses.UpdateAsync(warehouse, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Warehouse {Id} updated successfully for tenant {TenantId}",
                warehouse.Id,
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
                Country = warehouse.Country,
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
            _logger.LogError(ex, "Error updating warehouse {Id}", request.Id);
            return Result<WarehouseDto>.Failure("An error occurred while updating the warehouse");
        }
    }
}
