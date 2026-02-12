using Microsoft.EntityFrameworkCore;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.DTOs;
using SaaS.Domain.Entities;

namespace SaaS.Infrastructure.Persistence.Repositories;

public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.BillingCountry)
            .Include(c => c.ShippingCountry)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Customer?> GetByEmailAsync(string email, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.BillingCountry)
            .Include(c => c.ShippingCountry)
            .FirstOrDefaultAsync(
                c => c.Email == email && c.TenantId == tenantId && !c.IsDeleted,
                cancellationToken);
    }

    public async Task<Customer?> GetByTaxIdAsync(string taxId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.BillingCountry)
            .Include(c => c.ShippingCountry)
            .FirstOrDefaultAsync(
                c => c.TaxId == taxId && c.TenantId == tenantId && !c.IsDeleted,
                cancellationToken);
    }

    public async Task<List<Customer>> GetAllByTenantAsync(Guid tenantId, CustomerFilters? filters = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(c => c.BillingCountry)
            .Include(c => c.ShippingCountry)
            .Where(c => c.TenantId == tenantId && !c.IsDeleted);

        // Apply filters if provided
        if (filters != null)
        {
            // SearchTerm searches across multiple fields
            if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
            {
                var searchTerm = filters.SearchTerm.ToLower();
                query = query.Where(c =>
                    c.Name.ToLower().Contains(searchTerm) ||
                    c.Email.ToLower().Contains(searchTerm) ||
                    (c.Phone != null && c.Phone.ToLower().Contains(searchTerm)) ||
                    (c.ContactPerson != null && c.ContactPerson.ToLower().Contains(searchTerm)));
            }

            if (!string.IsNullOrWhiteSpace(filters.Name))
            {
                query = query.Where(c => c.Name.Contains(filters.Name));
            }

            if (!string.IsNullOrWhiteSpace(filters.Email))
            {
                query = query.Where(c => c.Email.Contains(filters.Email));
            }

            if (!string.IsNullOrWhiteSpace(filters.Phone))
            {
                query = query.Where(c => c.Phone == filters.Phone);
            }

            if (!string.IsNullOrWhiteSpace(filters.TaxId))
            {
                query = query.Where(c => c.TaxId == filters.TaxId);
            }

            if (!string.IsNullOrWhiteSpace(filters.City))
            {
                query = query.Where(c => c.BillingCity == filters.City || c.ShippingCity == filters.City);
            }

            if (!string.IsNullOrWhiteSpace(filters.Country))
            {
                query = query.Where(c =>
                    (c.BillingCountry != null && c.BillingCountry.Code == filters.Country) ||
                    (c.ShippingCountry != null && c.ShippingCountry.Code == filters.Country));
            }

            if (filters.IsActive.HasValue)
            {
                query = query.Where(c => c.IsActive == filters.IsActive.Value);
            }
        }

        return await query
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Customer>> GetActiveByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.BillingCountry)
            .Include(c => c.ShippingCountry)
            .Where(c => c.TenantId == tenantId && c.IsActive && !c.IsDeleted)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }
}
