namespace SaaS.Application.DTOs;

/// <summary>
/// Company user data transfer object with role information
/// </summary>
public class CompanyUserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public RoleDto Role { get; set; } = null!;
    public DateTime JoinedAt { get; set; }
    public bool IsActive { get; set; }
}
