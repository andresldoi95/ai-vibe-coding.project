using SaaS.Domain.Entities;

namespace SaaS.Application.Common.Interfaces;

/// <summary>
/// Repository interface for Stock Movement operations
/// </summary>
public interface IStockMovementRepository : IRepository<StockMovement>
{
    /// <summary>
    /// Get all stock movements for a tenant with product and warehouse details
    /// </summary>
    Task<List<StockMovement>> GetAllForTenantAsync(Guid tenantId);
    
    /// <summary>
    /// Get stock movements by product
    /// </summary>
    Task<List<StockMovement>> GetByProductIdAsync(Guid productId, Guid tenantId);
    
    /// <summary>
    /// Get stock movements by warehouse
    /// </summary>
    Task<List<StockMovement>> GetByWarehouseIdAsync(Guid warehouseId, Guid tenantId);
    
    /// <summary>
    /// Get stock movement by ID with related entities
    /// </summary>
    Task<StockMovement?> GetByIdWithDetailsAsync(Guid id, Guid tenantId);
}
