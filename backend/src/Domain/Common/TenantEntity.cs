namespace SaaS.Domain.Common;

/// <summary>
/// Base entity for tenant-specific data with automatic tenant isolation
/// </summary>
public abstract class TenantEntity : AuditableEntity
{
    public Guid TenantId { get; set; }
}
