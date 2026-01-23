using Beyond8.Common.Data.Implements;
using Beyond8.Identity.Domain.Entities;
using Beyond8.Identity.Domain.Repositories.Interfaces;
using Beyond8.Identity.Infrastructure.Data;

namespace Beyond8.Identity.Infrastructure.Repositories.Inplements;

public class RoleRepository(IdentityDbContext context) : PostgresRepository<Role>(context), IRoleRepository
{
    public async Task<Role?> FindByCodeAsync(string code)
    {
        return await FindOneAsync(r => r.Code == code);
    }
}
