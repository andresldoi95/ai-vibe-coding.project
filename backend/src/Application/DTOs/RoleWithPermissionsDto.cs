namespace SaaS.Application.DTOs;

/// <summary>
/// Role with permissions data transfer object
/// </summary>
public class RoleWithPermissionsDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Priority { get; set; }
    public bool IsSystemRole { get; set; }
    public bool IsActive { get; set; }
    public List<PermissionDto> Permissions { get; set; } = new();
    public int UserCount { get; set; }
}
