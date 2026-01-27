using Beyond8.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Identity.Infrastructure.Data.Seeders
{
    public static class SubscriptionPlanSeedData
    {
        public const string CodeFree = "FREE";
        public const string CodePlus = "PLUS";
        public const string CodePro = "PRO";

        public static async Task SeedSubscriptionPlansAsync(IdentityDbContext context)
        {
            if (await context.SubscriptionPlans.AnyAsync())
                return;

            var systemId = Guid.Empty;
            var now = DateTime.UtcNow;

            var plans = new List<SubscriptionPlan>
            {
                new()
                {
                    Id = Guid.CreateVersion7(),
                    Code = CodeFree,
                    Name = "Free",
                    Description = "7 ngày đầu sau khi đăng ký, 35 request trong 7 ngày.",
                    Price = 0,
                    Currency = "VND",
                    DurationDays = 0,
                    TotalRequestsInPeriod = 35,
                    MaxRequestsPerWeek = 35,
                    IsActive = true,
                    CreatedAt = now,
                    CreatedBy = systemId
                },
                new()
                {
                    Id = Guid.CreateVersion7(),
                    Code = CodePlus,
                    Name = "Plus",
                    Description = "Gói theo tháng, 200 request/tháng, tối đa 50 request/tuần.",
                    Price = 299, // Set actual price when integrating Sales
                    Currency = "VND",
                    DurationDays = 30,
                    TotalRequestsInPeriod = 200,
                    MaxRequestsPerWeek = 50,
                    IsActive = true,
                    CreatedAt = now,
                    CreatedBy = systemId
                },
                new()
                {
                    Id = Guid.CreateVersion7(),
                    Code = CodePro,
                    Name = "Pro",
                    Description = "Gói theo tháng, 400 request/tháng, tối đa 100 request/tuần.",
                    Price = 499, // Set actual price when integrating Sales
                    Currency = "VND",
                    DurationDays = 30,
                    TotalRequestsInPeriod = 400,
                    MaxRequestsPerWeek = 100,
                    IsActive = true,
                    CreatedAt = now,
                    CreatedBy = systemId
                }
            };

            await context.SubscriptionPlans.AddRangeAsync(plans);
            await context.SaveChangesAsync();
        }
    }
}
