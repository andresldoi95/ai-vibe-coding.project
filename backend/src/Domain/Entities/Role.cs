using SaaS.Domain.Common;

namespace SaaS.Domain.Entities;

/// <summary>
/// Represents a tenant-scoped role with assigned permissions
/// </summary>
public class Role : TenantEntity
{
    /// <summary>
    /// Role name (e.g., "Owner", "Admin", "Manager", "User")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Role description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Priority level for role hierarchy (higher = more powerful)
    /// Owner = 100, Admin = 50, Manager = 25, User = 10
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Whether this is a system-defined role (cannot be deleted)
    /// </summary>
    public bool IsSystemRole { get; set; } = false;

    /// <summary>
    /// Whether this role is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public ICollection<UserTenant> UserTenants { get; set; } = new List<UserTenant>();
}
