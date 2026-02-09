using Microsoft.EntityFrameworkCore;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;

namespace SaaS.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Warehouse Inventory operations
/// </summary>
public class WarehouseInventoryRepository : Repository<WarehouseInventory>, IWarehouseInventoryRepository
{
    public WarehouseInventoryRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<WarehouseInventory?> GetByProductAndWarehouseAsync(Guid productId, Guid warehouseId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.WarehouseInventory
            .Include(wi => wi.Product)
            .Include(wi => wi.Warehouse)
            .FirstOrDefaultAsync(wi =>
                wi.ProductId == productId &&
                wi.WarehouseId == warehouseId &&
                wi.TenantId == tenantId,
                cancellationToken);
    }

    public async Task<List<WarehouseInventory>> GetByProductIdAsync(Guid productId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.WarehouseInventory
            .Include(wi => wi.Product)
            .Include(wi => wi.Warehouse)
            .Where(wi => wi.ProductId == productId && wi.TenantId == tenantId)
            .OrderBy(wi => wi.Warehouse.Name)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<WarehouseInventory>> GetByWarehouseIdAsync(Guid warehouseId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.WarehouseInventory
            .Include(wi => wi.Product)
            .Include(wi => wi.Warehouse)
            .Where(wi => wi.WarehouseId == warehouseId && wi.TenantId == tenantId)
            .OrderBy(wi => wi.Product.Name)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<WarehouseInventory>> GetLowStockItemsAsync(Guid tenantId, int threshold = 10, CancellationToken cancellationToken = default)
    {
        return await _context.WarehouseInventory
            .Include(wi => wi.Product)
            .Include(wi => wi.Warehouse)
            .Where(wi => wi.TenantId == tenantId && wi.Quantity <= threshold)
            .OrderBy(wi => wi.Quantity)
            .ThenBy(wi => wi.Product.Name)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<WarehouseInventory> UpsertAsync(Guid productId, Guid warehouseId, Guid tenantId, int quantityDelta, DateTime? movementDate = null, CancellationToken cancellationToken = default)
    {
        var inventory = await _context.WarehouseInventory
            .FirstOrDefaultAsync(wi =>
                wi.ProductId == productId &&
                wi.WarehouseId == warehouseId &&
                wi.TenantId == tenantId,
                cancellationToken);

        if (inventory == null)
        {
            // Create new inventory record
            inventory = new WarehouseInventory
            {
                ProductId = productId,
                WarehouseId = warehouseId,
                TenantId = tenantId,
                Quantity = quantityDelta,
                ReservedQuantity = 0,
                LastMovementDate = movementDate ?? DateTime.UtcNow
            };
            await _context.WarehouseInventory.AddAsync(inventory, cancellationToken);
        }
        else
        {
            // Update existing inventory
            inventory.Quantity += quantityDelta;
            inventory.LastMovementDate = movementDate ?? DateTime.UtcNow;
            inventory.UpdatedAt = DateTime.UtcNow;
        }

        return inventory;
    }

    public async Task<int> GetTotalStockByProductIdAsync(Guid productId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.WarehouseInventory
            .Where(wi => wi.ProductId == productId && wi.TenantId == tenantId)
            .SumAsync(wi => wi.Quantity, cancellationToken);
    }

    public async Task<Dictionary<Guid, int>> GetTotalStockByProductIdsAsync(List<Guid> productIds, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.WarehouseInventory
            .Where(wi => productIds.Contains(wi.ProductId) && wi.TenantId == tenantId)
            .GroupBy(wi => wi.ProductId)
            .Select(g => new { ProductId = g.Key, TotalQuantity = g.Sum(wi => wi.Quantity) })
            .ToDictionaryAsync(x => x.ProductId, x => x.TotalQuantity, cancellationToken);
    }
}
