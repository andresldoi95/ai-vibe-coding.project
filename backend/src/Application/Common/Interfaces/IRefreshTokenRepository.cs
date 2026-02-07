using SaaS.Domain.Entities;

namespace SaaS.Application.Common.Interfaces;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task RevokeTokenAsync(RefreshToken token, string ipAddress, CancellationToken cancellationToken = default);
}
