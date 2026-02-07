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
    RefreshToken GenerateRefreshToken(string ipAddress);
}
