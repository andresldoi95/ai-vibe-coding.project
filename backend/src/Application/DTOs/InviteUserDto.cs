namespace SaaS.Application.DTOs;

/// <summary>
/// DTO for inviting a user to a company
/// </summary>
public class InviteUserDto
{
    public string Email { get; set; } = string.Empty;
    public Guid RoleId { get; set; }
    public string? PersonalMessage { get; set; }
}
