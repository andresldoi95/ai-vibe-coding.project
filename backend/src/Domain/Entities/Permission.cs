using SaaS.Domain.Common;

namespace SaaS.Domain.Entities;

/// <summary>
/// Represents a global permission that can be assigned to roles
/// </summary>
public class Permission : BaseEntity
{
    /// <summary>
    /// Unique permission name (e.g., "warehouses.create")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Resource category (e.g., "warehouses", "users", "tenants")
    /// </summary>
    public string Resource { get; set; } = string.Empty;

    /// <summary>
    /// Action type (e.g., "read", "create", "update", "delete")
    /// </summary>
    public string Action { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
