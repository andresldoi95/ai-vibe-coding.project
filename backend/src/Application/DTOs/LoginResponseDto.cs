namespace SaaS.Application.DTOs;

/// <summary>
/// Login response matching frontend expectations
/// </summary>
public class LoginResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public UserDto User { get; set; } = null!;
    public List<TenantDto> Tenants { get; set; } = new();
}
