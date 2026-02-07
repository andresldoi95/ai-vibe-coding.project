using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;

namespace SaaS.Infrastructure.Persistence.Repositories;

public class UserTenantRepository : Repository<UserTenant>, IUserTenantRepository
{
    public UserTenantRepository(ApplicationDbContext context) : base(context)
    {
    }
}
