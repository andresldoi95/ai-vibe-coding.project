using SaaS.Domain.Common;

namespace SaaS.Domain.Entities;

/// <summary>
/// User entity representing application users
/// </summary>
public class User : AuditableEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool EmailConfirmed { get; set; } = false;

    // Password Reset
    public string? ResetToken { get; set; }
    public DateTime? ResetTokenExpiry { get; set; }

    // Navigation property
    public ICollection<UserTenant> UserTenants { get; set; } = new List<UserTenant>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
