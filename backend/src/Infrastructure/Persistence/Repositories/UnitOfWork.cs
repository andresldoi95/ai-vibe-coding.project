using Microsoft.EntityFrameworkCore.Storage;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;

namespace SaaS.Infrastructure.Persistence.Repositories;

/// <summary>
/// Unit of Work implementation coordinating all repositories
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    public IUserRepository Users { get; }
    public ITenantRepository Tenants { get; }
    public IUserTenantRepository UserTenants { get; }
    public IRefreshTokenRepository RefreshTokens { get; }
    public IWarehouseRepository Warehouses { get; }
    public IProductRepository Products { get; }
    public ICustomerRepository Customers { get; }
    public IStockMovementRepository StockMovements { get; }
    public IWarehouseInventoryRepository WarehouseInventory { get; }
    public IRoleRepository Roles { get; }
    public IPermissionRepository Permissions { get; }
    public ITaxRateRepository TaxRates { get; }
    public IInvoiceConfigurationRepository InvoiceConfigurations { get; }
    public IInvoiceRepository Invoices { get; }
    public IRepository<InvoiceItem> InvoiceItems { get; }

    public UnitOfWork(
        ApplicationDbContext context,
        IUserRepository userRepository,
        ITenantRepository tenantRepository,
        IUserTenantRepository userTenantRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IWarehouseRepository warehouseRepository,
        IProductRepository productRepository,
        ICustomerRepository customerRepository,
        IStockMovementRepository stockMovementRepository,
        IWarehouseInventoryRepository warehouseInventoryRepository,
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        ITaxRateRepository taxRateRepository,
        IInvoiceConfigurationRepository invoiceConfigurationRepository,
        IInvoiceRepository invoiceRepository,
        IRepository<InvoiceItem> invoiceItemRepository)
    {
        _context = context;
        Users = userRepository;
        Tenants = tenantRepository;
        UserTenants = userTenantRepository;
        RefreshTokens = refreshTokenRepository;
        Warehouses = warehouseRepository;
        Products = productRepository;
        Customers = customerRepository;
        TaxRates = taxRateRepository;
        InvoiceConfigurations = invoiceConfigurationRepository;
        Invoices = invoiceRepository;
        InvoiceItems = invoiceItemRepository;
        StockMovements = stockMovementRepository;
        WarehouseInventory = warehouseInventoryRepository;
        Roles = roleRepository;
        Permissions = permissionRepository;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
