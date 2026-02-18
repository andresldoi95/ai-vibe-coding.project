using SaaS.Domain.Common;

namespace SaaS.Domain.Entities;

/// <summary>
/// Represents an invitation for a user to join a tenant
/// </summary>
public class UserInvitation : BaseEntity
{
    public string InvitationToken { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public Guid InvitedByUserId { get; set; }
    public User InvitedByUser { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public bool IsActive { get; set; } = true;
}
