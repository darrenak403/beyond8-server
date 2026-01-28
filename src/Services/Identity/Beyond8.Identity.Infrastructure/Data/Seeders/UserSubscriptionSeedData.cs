using Beyond8.Identity.Domain.Entities;
using Beyond8.Identity.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Identity.Infrastructure.Data.Seeders
{
    public static class UserSubscriptionSeedData
    {
        /// <summary>Instructor seed user ID (UserWithRoleSeedData userIds[3]).</summary>
        private static readonly Guid InstructorSeedUserId = new("00000000-0000-0000-0000-000000000006");

        public static async Task SeedUserSubscriptionsAsync(IdentityDbContext context)
        {
            if (await context.UserSubscriptions.AnyAsync())
                return;

            var freePlan = await context.SubscriptionPlans
                .FirstOrDefaultAsync(p => p.Code == SubscriptionPlanSeedData.CodeFree);
            var ultraPlan = await context.SubscriptionPlans
                .FirstOrDefaultAsync(p => p.Code == SubscriptionPlanSeedData.CodeUltra);
            if (freePlan == null)
                return;

            var users = await context.Users.ToListAsync();
            var now = DateTime.UtcNow;

            var subscriptions = new List<UserSubscription>();
            foreach (var user in users)
            {
                var isInstructor = user.Id == InstructorSeedUserId;
                var plan = isInstructor && ultraPlan != null ? ultraPlan : freePlan;
                var (totalRemaining, remainingPerWeek, expiresAt) = isInstructor && ultraPlan != null
                    ? (800, 200, now.AddDays(30))
                    : (35, 35, now.AddDays(7));

                subscriptions.Add(new UserSubscription
                {
                    UserId = user.Id,
                    PlanId = plan.Id,
                    StartedAt = now,
                    ExpiresAt = expiresAt,
                    Status = SubscriptionStatus.Active,
                    TotalRemainingRequests = totalRemaining,
                    RemainingRequestsPerWeek = remainingPerWeek,
                    RequestLimitedEndsAt = null,
                    CreatedAt = now,
                    CreatedBy = user.Id
                });
            }

            await context.UserSubscriptions.AddRangeAsync(subscriptions);
            await context.SaveChangesAsync();
        }
    }
}
