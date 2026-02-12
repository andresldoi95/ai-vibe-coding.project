using Microsoft.EntityFrameworkCore;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;

namespace SaaS.Infrastructure.Persistence;

/// <summary>
/// Main application database context
/// </summary>
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ITenantContext _tenantContext;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ITenantContext tenantContext) : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<UserTenant> UserTenants => Set<UserTenant>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<WarehouseInventory> WarehouseInventory => Set<WarehouseInventory>();
    public DbSet<EmailLog> EmailLogs => Set<EmailLog>();
    public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<TaxRate> TaxRates => Set<TaxRate>();
    public DbSet<InvoiceConfiguration> InvoiceConfigurations => Set<InvoiceConfiguration>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<Country> Countries => Set<Country>();

    // SRI Ecuador entities
    public DbSet<Establishment> Establishments => Set<Establishment>();
    public DbSet<EmissionPoint> EmissionPoints => Set<EmissionPoint>();
    public DbSet<SriConfiguration> SriConfigurations => Set<SriConfiguration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global query filter for soft deletes
        modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<Tenant>().HasQueryFilter(t => !t.IsDeleted);
        modelBuilder.Entity<Warehouse>().HasQueryFilter(w => !w.IsDeleted);
        modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
        modelBuilder.Entity<Customer>().HasQueryFilter(c => !c.IsDeleted);
        modelBuilder.Entity<StockMovement>().HasQueryFilter(s => !s.IsDeleted);
        modelBuilder.Entity<Establishment>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<EmissionPoint>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<SriConfiguration>().HasQueryFilter(s => !s.IsDeleted);
        modelBuilder.Entity<Invoice>().HasQueryFilter(i => !i.IsDeleted);
        modelBuilder.Entity<TaxRate>().HasQueryFilter(t => !t.IsDeleted);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update audit fields automatically
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is Domain.Common.AuditableEntity &&
                       (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            var entity = (Domain.Common.AuditableEntity)entityEntry.Entity;

            if (entityEntry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
