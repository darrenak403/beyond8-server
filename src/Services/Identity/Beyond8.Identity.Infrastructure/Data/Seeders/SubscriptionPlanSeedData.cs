using Beyond8.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Identity.Infrastructure.Data.Seeders
{
    public static class SubscriptionPlanSeedData
    {
        public const string CodeFree = "FREE";
        public const string CodePlus = "PLUS";
        public const string CodePro = "PRO";
        public const string CodeUltra = "ULTRA";

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
                    Includes =
                    [
                        "Không yêu cầu thanh toán",
                        "Giới hạn 35 request/tuần đầu tiên",
                        "Tất cả các tính năng AI cơ bản",
                        "Hỗ trợ email 24/7",
                    ],
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
                    Price = 299,
                    Currency = "VND",
                    DurationDays = 30,
                    TotalRequestsInPeriod = 200,
                    MaxRequestsPerWeek = 50,
                    Includes =
                    [
                        "200 request AI/tháng",
                        "Tối đa 50 request/tuần",
                        "Mở rộng thêm các tính năng AI",
                        "Hỗ trợ ưu tiên 24/7"
                    ],
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
                    Price = 499,
                    Currency = "VND",
                    DurationDays = 30,
                    TotalRequestsInPeriod = 400,
                    MaxRequestsPerWeek = 100,
                    Includes =
                    [
                        "400 request AI/tháng",
                        "Tối đa 100 request/tuần",
                        "Tất cả tính năng Plus",
                        "Hỗ trợ ưu tiên 24/7"
                    ],
                    IsActive = true,
                    CreatedAt = now,
                    CreatedBy = systemId
                },
                new()
                {
                    Id = Guid.CreateVersion7(),
                    Code = CodeUltra,
                    Name = "Ultra",
                    Description = "Gói theo tháng, 800 request/tháng, tối đa 200 request/tuần.",
                    Price = 899,
                    Currency = "VND",
                    DurationDays = 30,
                    TotalRequestsInPeriod = 800,
                    MaxRequestsPerWeek = 200,
                    Includes =
                    [
                        "800 request AI/tháng",
                        "Tối đa 200 request/tuần",
                        "Tất cả tính năng Pro",
                        "Hỗ trợ ưu tiên 24/7"
                    ],
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
