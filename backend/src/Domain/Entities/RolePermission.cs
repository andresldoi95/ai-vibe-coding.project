using SaaS.Domain.Common;

namespace SaaS.Domain.Entities;

/// <summary>
/// Junction table linking roles to permissions
/// </summary>
public class RolePermission : BaseEntity
{
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public Guid PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;
}
