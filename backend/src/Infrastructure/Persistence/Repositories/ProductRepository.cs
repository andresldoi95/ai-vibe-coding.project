using Microsoft.EntityFrameworkCore;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.DTOs;
using SaaS.Domain.Entities;

namespace SaaS.Infrastructure.Persistence.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Product?> GetByCodeAsync(string code, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(
                p => p.Code == code && p.TenantId == tenantId && !p.IsDeleted,
                cancellationToken);
    }

    public async Task<Product?> GetBySKUAsync(string sku, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(
                p => p.SKU == sku && p.TenantId == tenantId && !p.IsDeleted,
                cancellationToken);
    }

    public async Task<List<Product>> GetAllByTenantAsync(Guid tenantId, ProductFilters? filters = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Where(p => p.TenantId == tenantId && !p.IsDeleted);

        // Apply filters if provided
        if (filters != null)
        {
            if (!string.IsNullOrWhiteSpace(filters.Name))
            {
                query = query.Where(p => p.Name.Contains(filters.Name));
            }

            if (!string.IsNullOrWhiteSpace(filters.Code))
            {
                query = query.Where(p => p.Code.Contains(filters.Code));
            }

            if (!string.IsNullOrWhiteSpace(filters.SKU))
            {
                query = query.Where(p => p.SKU == filters.SKU);
            }

            if (!string.IsNullOrWhiteSpace(filters.Category))
            {
                query = query.Where(p => p.Category == filters.Category);
            }

            if (!string.IsNullOrWhiteSpace(filters.Brand))
            {
                query = query.Where(p => p.Brand == filters.Brand);
            }

            if (filters.IsActive.HasValue)
            {
                query = query.Where(p => p.IsActive == filters.IsActive.Value);
            }

            if (filters.MinPrice.HasValue)
            {
                query = query.Where(p => p.UnitPrice >= filters.MinPrice.Value);
            }

            if (filters.MaxPrice.HasValue)
            {
                query = query.Where(p => p.UnitPrice <= filters.MaxPrice.Value);
            }

            if (filters.LowStockOnly.HasValue && filters.LowStockOnly.Value)
            {
                query = query.Where(p => p.CurrentStockLevel.HasValue && 
                                       p.CurrentStockLevel.Value <= p.MinimumStockLevel);
            }
        }

        return await query
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Product>> GetActiveByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.TenantId == tenantId && p.IsActive && !p.IsDeleted)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }
}
