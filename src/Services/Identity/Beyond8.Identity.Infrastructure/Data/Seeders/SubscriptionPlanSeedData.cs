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
                        "35 request AI trong 7 ngày",
                        "Đánh giá hồ sơ giảng viên",
                        "Tạo quiz từ AI (cơ bản)"
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
                    Price = 299, // Set actual price when integrating Sales
                    Currency = "VND",
                    DurationDays = 30,
                    TotalRequestsInPeriod = 200,
                    MaxRequestsPerWeek = 50,
                    Includes =
                    [
                        "200 request AI/tháng",
                        "Tối đa 50 request/tuần",
                        "Đánh giá hồ sơ giảng viên",
                        "Tạo quiz từ AI",
                        "Hỗ trợ ưu tiên"
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
                    Price = 499, // Set actual price when integrating Sales
                    Currency = "VND",
                    DurationDays = 30,
                    TotalRequestsInPeriod = 400,
                    MaxRequestsPerWeek = 100,
                    Includes =
                    [
                        "400 request AI/tháng",
                        "Tối đa 100 request/tuần",
                        "Đánh giá hồ sơ giảng viên",
                        "Tạo quiz từ AI",
                        "Hỗ trợ ưu tiên",
                        "Xuất báo cáo"
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
                    Price = 899, // Set actual price when integrating Sales
                    Currency = "VND",
                    DurationDays = 30,
                    TotalRequestsInPeriod = 800,
                    MaxRequestsPerWeek = 200,
                    Includes =
                    [
                        "800 request AI/tháng",
                        "Tối đa 200 request/tuần",
                        "Tất cả tính năng Pro",
                        "Hỗ trợ chuyên biệt",
                        "API riêng (khi có)"
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
