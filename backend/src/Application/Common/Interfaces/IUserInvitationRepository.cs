using SaaS.Domain.Entities;

namespace SaaS.Application.Common.Interfaces;

public interface IUserInvitationRepository : IRepository<UserInvitation>
{
    Task<UserInvitation?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<UserInvitation?> GetActiveByEmailAndTenantAsync(string email, Guid tenantId, CancellationToken cancellationToken = default);
}
