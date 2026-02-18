namespace SaaS.Application.DTOs;

/// <summary>
/// DTO for updating a user's role in a company
/// </summary>
public class UpdateUserRoleDto
{
    public Guid UserId { get; set; }
    public Guid NewRoleId { get; set; }
}
