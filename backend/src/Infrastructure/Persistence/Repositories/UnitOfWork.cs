using Microsoft.EntityFrameworkCore.Storage;
using SaaS.Application.Common.Interfaces;

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
    public IRoleRepository Roles { get; }
    public IPermissionRepository Permissions { get; }

    public UnitOfWork(
        ApplicationDbContext context,
        IUserRepository userRepository,
        ITenantRepository tenantRepository,
        IUserTenantRepository userTenantRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IWarehouseRepository warehouseRepository,
        IProductRepository productRepository,
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository)
    {
        _context = context;
        Users = userRepository;
        Tenants = tenantRepository;
        UserTenants = userTenantRepository;
        RefreshTokens = refreshTokenRepository;
        Warehouses = warehouseRepository;
        Products = productRepository;
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
