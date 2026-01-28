using Beyond8.Identity.Domain.Entities;
using Beyond8.Identity.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Identity.Infrastructure.Data.Seeders
{
    public static class UserSubscriptionSeedData
    {
        public static async Task SeedUserSubscriptionsAsync(IdentityDbContext context)
        {
            if (await context.UserSubscriptions.AnyAsync())
                return;

            var freePlan = await context.SubscriptionPlans
                .FirstOrDefaultAsync(p => p.Code == SubscriptionPlanSeedData.CodeFree);
            if (freePlan == null)
                return;

            var users = await context.Users.ToListAsync();

            var now = DateTime.UtcNow;
            var subscriptions = users.Select(user => new UserSubscription
            {
                UserId = user.Id,
                PlanId = freePlan.Id,
                StartedAt = now,
                ExpiresAt = now.AddDays(7),
                Status = SubscriptionStatus.Active,
                TotalRemainingRequests = 35,
                RemainingRequestsPerWeek = 35,
                RequestLimitedEndsAt = null,
                CreatedAt = now,
                CreatedBy = user.Id
            }).ToList();

            await context.UserSubscriptions.AddRangeAsync(subscriptions);
            await context.SaveChangesAsync();
        }
    }
}
