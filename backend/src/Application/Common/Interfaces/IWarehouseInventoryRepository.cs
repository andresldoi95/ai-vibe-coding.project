using SaaS.Domain.Entities;

namespace SaaS.Application.Common.Interfaces;

/// <summary>
/// Repository interface for Warehouse Inventory operations
/// </summary>
public interface IWarehouseInventoryRepository : IRepository<WarehouseInventory>
{
    /// <summary>
    /// Get inventory record for a specific product in a warehouse
    /// </summary>
    Task<WarehouseInventory?> GetByProductAndWarehouseAsync(Guid productId, Guid warehouseId, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all inventory records for a product across all warehouses
    /// </summary>
    Task<List<WarehouseInventory>> GetByProductIdAsync(Guid productId, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all inventory records for a warehouse
    /// </summary>
    Task<List<WarehouseInventory>> GetByWarehouseIdAsync(Guid warehouseId, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get low stock items (quantity below threshold)
    /// </summary>
    Task<List<WarehouseInventory>> GetLowStockItemsAsync(Guid tenantId, int threshold = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update or create inventory record for a product in a warehouse
    /// </summary>
    Task<WarehouseInventory> UpsertAsync(Guid productId, Guid warehouseId, Guid tenantId, int quantityDelta, DateTime? movementDate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get total stock quantity for a product across all warehouses
    /// </summary>
    Task<int> GetTotalStockByProductIdAsync(Guid productId, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get total stock quantities for multiple products
    /// </summary>
    Task<Dictionary<Guid, int>> GetTotalStockByProductIdsAsync(List<Guid> productIds, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all warehouse inventory records with warehouse and product details for a tenant
    /// </summary>
    Task<List<WarehouseInventory>> GetAllWithDetailsForTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
