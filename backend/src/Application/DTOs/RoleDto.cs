namespace SaaS.Application.DTOs;

/// <summary>
/// Role data transfer object
/// </summary>
public class RoleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Priority { get; set; }
}
