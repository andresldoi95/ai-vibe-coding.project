namespace SaaS.Application.DTOs;

/// <summary>
/// DTO for accepting an invitation
/// </summary>
public class AcceptInvitationDto
{
    public string InvitationToken { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Password { get; set; }
}
