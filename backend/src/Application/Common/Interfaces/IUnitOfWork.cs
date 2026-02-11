using SaaS.Domain.Entities;

namespace SaaS.Application.Common.Interfaces;

/// <summary>
/// Unit of Work pattern for coordinating repository operations
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    ITenantRepository Tenants { get; }
    IUserTenantRepository UserTenants { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    IWarehouseRepository Warehouses { get; }
    IProductRepository Products { get; }
    ICustomerRepository Customers { get; }
    IStockMovementRepository StockMovements { get; }
    IWarehouseInventoryRepository WarehouseInventory { get; }
    IRoleRepository Roles { get; }
    IPermissionRepository Permissions { get; }
    ITaxRateRepository TaxRates { get; }
    IInvoiceConfigurationRepository InvoiceConfigurations { get; }
    IInvoiceRepository Invoices { get; }
    IRepository<InvoiceItem> InvoiceItems { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
