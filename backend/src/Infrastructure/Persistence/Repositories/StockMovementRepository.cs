using Microsoft.EntityFrameworkCore;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;

namespace SaaS.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Stock Movement operations
/// </summary>
public class StockMovementRepository : Repository<StockMovement>, IStockMovementRepository
{
    public StockMovementRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<StockMovement>> GetAllForTenantAsync(Guid tenantId)
    {
        return await _context.StockMovements
            .Include(sm => sm.Product)
            .Include(sm => sm.Warehouse)
            .Include(sm => sm.DestinationWarehouse)
            .Where(sm => sm.TenantId == tenantId)
            .OrderByDescending(sm => sm.MovementDate)
            .ThenByDescending(sm => sm.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<StockMovement>> GetByProductIdAsync(Guid productId, Guid tenantId)
    {
        return await _context.StockMovements
            .Include(sm => sm.Product)
            .Include(sm => sm.Warehouse)
            .Include(sm => sm.DestinationWarehouse)
            .Where(sm => sm.ProductId == productId && sm.TenantId == tenantId)
            .OrderByDescending(sm => sm.MovementDate)
            .ThenByDescending(sm => sm.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<StockMovement>> GetByWarehouseIdAsync(Guid warehouseId, Guid tenantId)
    {
        return await _context.StockMovements
            .Include(sm => sm.Product)
            .Include(sm => sm.Warehouse)
            .Include(sm => sm.DestinationWarehouse)
            .Where(sm => (sm.WarehouseId == warehouseId || sm.DestinationWarehouseId == warehouseId) 
                && sm.TenantId == tenantId)
            .OrderByDescending(sm => sm.MovementDate)
            .ThenByDescending(sm => sm.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<StockMovement?> GetByIdWithDetailsAsync(Guid id, Guid tenantId)
    {
        return await _context.StockMovements
            .Include(sm => sm.Product)
            .Include(sm => sm.Warehouse)
            .Include(sm => sm.DestinationWarehouse)
            .FirstOrDefaultAsync(sm => sm.Id == id && sm.TenantId == tenantId);
    }

    public async Task<List<StockMovement>> GetForExportAsync(
        Guid tenantId,
        string? brand = null,
        string? category = null,
        Guid? warehouseId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.StockMovements
            .Include(sm => sm.Product)
            .Include(sm => sm.Warehouse)
            .Include(sm => sm.DestinationWarehouse)
            .Where(sm => sm.TenantId == tenantId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(brand))
        {
            query = query.Where(sm => sm.Product.Brand == brand);
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(sm => sm.Product.Category == category);
        }

        if (warehouseId.HasValue)
        {
            query = query.Where(sm =>
                sm.WarehouseId == warehouseId.Value ||
                sm.DestinationWarehouseId == warehouseId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(sm => sm.MovementDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(sm => sm.MovementDate <= toDate.Value);
        }

        return await query
            .OrderByDescending(sm => sm.MovementDate)
            .ThenByDescending(sm => sm.CreatedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
