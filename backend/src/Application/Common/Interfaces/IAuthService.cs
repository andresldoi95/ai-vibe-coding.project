using SaaS.Domain.Entities;

namespace SaaS.Application.Common.Interfaces;

/// <summary>
/// Authentication service for user login and token management
/// </summary>
public interface IAuthService
{
    Task<User?> ValidateCredentialsAsync(string email, string password);
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
    string GenerateJwtToken(User user, List<Guid> tenantIds);
    string GenerateJwtTokenWithRole(User user, Guid tenantId, Role role, List<string> permissions);
    RefreshToken GenerateRefreshToken(string ipAddress);
}
