using Beyond8.Common.Data.Implements;
using Beyond8.Identity.Domain.Entities;
using Beyond8.Identity.Domain.Repositories.Interfaces;
using Beyond8.Identity.Infrastructure.Data;

namespace Beyond8.Identity.Infrastructure.Repositories.Inplements
{
    public class SubscriptionPlanRepository(IdentityDbContext context)
        : PostgresRepository<SubscriptionPlan>(context), ISubscriptionPlanRepository
    {
        public async Task<SubscriptionPlan?> FindByCodeAsync(string code)
        {
            return await FindOneAsync(p => p.Code == code);
        }
    }
}
