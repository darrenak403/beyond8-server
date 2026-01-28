using Beyond8.Common.Data.Implements;
using Beyond8.Identity.Domain.Entities;
using Beyond8.Identity.Domain.Enums;
using Beyond8.Identity.Domain.Repositories.Interfaces;
using Beyond8.Identity.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Identity.Infrastructure.Repositories.Inplements
{
    public class UserSubscriptionRepository(IdentityDbContext context)
        : PostgresRepository<UserSubscription>(context), IUserSubscriptionRepository
    {
        public async Task<UserSubscription?> GetActiveByUserIdAsync(Guid userId)
        {
            var now = DateTime.UtcNow;
            return await AsQueryable()
                .Include(s => s.Plan)
                .Where(s => s.UserId == userId
                    && s.Status == SubscriptionStatus.Active
                    && (s.ExpiresAt == null || s.ExpiresAt > now))
                .OrderByDescending(s => s.ExpiresAt)
                .FirstOrDefaultAsync();
        }
    }
}
