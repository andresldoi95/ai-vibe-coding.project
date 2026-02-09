namespace SaaS.Application.DTOs;

/// <summary>
/// Response DTO for tenant selection
/// </summary>
public class SelectTenantResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public RoleDto Role { get; set; } = null!;
    public List<string> Permissions { get; set; } = new();
}
