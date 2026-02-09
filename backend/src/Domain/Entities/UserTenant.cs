using SaaS.Domain.Common;
using SaaS.Domain.Enums;

namespace SaaS.Domain.Entities;

/// <summary>
/// Join table representing user-tenant relationships with roles
/// </summary>
public class UserTenant : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    // New role system (FK to Roles table)
    // Nullable during migration - migration will populate this
    public Guid? RoleId { get; set; }
    public Role? Role { get; set; }

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}
