using Microsoft.EntityFrameworkCore;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;

namespace SaaS.Infrastructure.Persistence.Repositories;

public class PaymentRepository : Repository<Payment>, IPaymentRepository
{
    public PaymentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Invoice)
                .ThenInclude(i => i.Customer)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);
    }

    public async Task<List<Payment>> GetAllByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Invoice)
                .ThenInclude(i => i.Customer)
            .Where(p => p.TenantId == tenantId && !p.IsDeleted)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Payment>> GetByInvoiceIdAsync(Guid invoiceId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Invoice)
                .ThenInclude(i => i.Customer)
            .Where(p => p.InvoiceId == invoiceId && p.TenantId == tenantId && !p.IsDeleted)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Payment>> GetByStatusAsync(PaymentStatus status, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Invoice)
                .ThenInclude(i => i.Customer)
            .Where(p => p.Status == status && p.TenantId == tenantId && !p.IsDeleted)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(cancellationToken);
    }
}
