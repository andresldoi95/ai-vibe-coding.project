using SaaS.Domain.Common;
using SaaS.Domain.Enums;

namespace SaaS.Domain.Entities;

/// <summary>
/// Tenant (Company) entity representing isolated customer environments
/// </summary>
public class Tenant : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public TenantStatus Status { get; set; } = TenantStatus.Active;
    public string? ConnectionString { get; set; }
    public string? SchemaName { get; set; }

    // Navigation property
    public ICollection<UserTenant> UserTenants { get; set; } = new List<UserTenant>();
}
