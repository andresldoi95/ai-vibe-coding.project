using Microsoft.EntityFrameworkCore;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;

namespace SaaS.Infrastructure.Persistence.Repositories;

public class PermissionRepository : IPermissionRepository
{
    private readonly ApplicationDbContext _context;

    public PermissionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Permission>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Permissions
            .OrderBy(p => p.Resource)
            .ThenBy(p => p.Action)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Permission>> GetByIdsAsync(List<Guid> ids, CancellationToken cancellationToken = default)
    {
        return await _context.Permissions
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(cancellationToken);
    }
}
