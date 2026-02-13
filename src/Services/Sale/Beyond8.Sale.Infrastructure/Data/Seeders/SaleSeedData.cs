using Beyond8.Sale.Domain.Entities;
using Beyond8.Sale.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Sale.Infrastructure.Data.Seeders;

public static class SaleSeedData
{
    public static async Task SeedCouponsAsync(SaleDbContext context)
    {
        if (await context.Coupons.AnyAsync())
            return;

        var systemId = new Guid("00000000-0000-0000-0000-000000000001");
        var now = DateTime.UtcNow;

        // IDs from Identity service seed data
        var instructorId1 = new Guid("00000000-0000-0000-0000-000000000006"); // Instructor 1

        var coupons = new List<Coupon>
        {
            // ═══════════════════════════════════════════════════════════
            // Admin Coupons (Platform-Wide)
            // ═══════════════════════════════════════════════════════════

            new()
            {
                Id = Guid.CreateVersion7(),
                Code = "WELCOME50K",
                Description = "Giảm 50,000 VND cho đơn hàng đầu tiên từ 500,000 VND",
                Type = CouponType.FixedAmount,
                Value = 50000,
                MinOrderAmount = 500000,
                MaxDiscountAmount = null,
                UsageLimit = 1000,
                UsagePerUser = 1,
                UsedCount = 0,
                ApplicableInstructorId = null, // System coupon
                ApplicableCourseId = null,
                ValidFrom = now,
                ValidTo = now.AddMonths(3),
                IsActive = true,
                CreatedAt = now,
                CreatedBy = systemId
            },

            new()
            {
                Id = Guid.CreateVersion7(),
                Code = "SALE20",
                Description = "Giảm 20% cho tất cả khóa học (tối đa 200,000 VND)",
                Type = CouponType.Percentage,
                Value = 20,
                MinOrderAmount = 300000,
                MaxDiscountAmount = 200000,
                UsageLimit = 500,
                UsagePerUser = 2,
                UsedCount = 0,
                ApplicableInstructorId = null, // System coupon
                ApplicableCourseId = null,
                ValidFrom = now,
                ValidTo = now.AddMonths(1),
                IsActive = true,
                CreatedAt = now,
                CreatedBy = systemId
            },

            new()
            {
                Id = Guid.CreateVersion7(),
                Code = "FREESHIP100K",
                Description = "Giảm 100,000 VND cho đơn từ 2 triệu",
                Type = CouponType.FixedAmount,
                Value = 100000,
                MinOrderAmount = 2000000,
                MaxDiscountAmount = null,
                UsageLimit = null, // Unlimited
                UsagePerUser = null, // Unlimited per user
                UsedCount = 0,
                ApplicableInstructorId = null, // System coupon
                ApplicableCourseId = null,
                ValidFrom = now,
                ValidTo = now.AddMonths(6),
                IsActive = true,
                CreatedAt = now,
                CreatedBy = systemId
            },

            new()
            {
                Id = Guid.CreateVersion7(),
                Code = "UUDAI100K",
                Description = "Giảm 10% cho đơn hàng từ 100,000 VNĐ trở lên",
                Type = CouponType.Percentage,
                Value = 10,
                MinOrderAmount = 100000,
                MaxDiscountAmount = null,
                UsageLimit = 200,
                UsagePerUser = 1,
                UsedCount = 0,
                ApplicableInstructorId = null, // System coupon
                ApplicableCourseId = null,
                ValidFrom = new DateTime(now.Year, 2, 11, 0, 0, 0, DateTimeKind.Utc),
                ValidTo = new DateTime(now.Year, 9, 30, 23, 59, 59, DateTimeKind.Utc),
                IsActive = true,
                CreatedAt = now,
                CreatedBy = systemId
            },

            // ═══════════════════════════════════════════════════════════
            // Instructor Coupons (Instructor-Specific)
            // ═══════════════════════════════════════════════════════════

            new()
            {
                Id = Guid.CreateVersion7(),
                Code = "INSTRUCTOR20",
                Description = "Giảm 20% cho tất cả khóa học của giảng viên (tối đa 300,000 VND)",
                Type = CouponType.Percentage,
                Value = 20,
                MinOrderAmount = 500000,
                MaxDiscountAmount = 300000,
                UsageLimit = 100,
                UsagePerUser = 1,
                UsedCount = 0,
                ApplicableInstructorId = instructorId1, // Instructor-specific
                ApplicableCourseId = null,
                ValidFrom = now,
                ValidTo = now.AddMonths(2),
                IsActive = true,
                CreatedAt = now,
                CreatedBy = instructorId1
            },

            new()
            {
                Id = Guid.CreateVersion7(),
                Code = "TEACHER15",
                Description = "Giảm 15% cho khóa học của giảng viên (tối đa 150,000 VND)",
                Type = CouponType.Percentage,
                Value = 15,
                MinOrderAmount = 300000,
                MaxDiscountAmount = 150000,
                UsageLimit = 50,
                UsagePerUser = 1,
                UsedCount = 0,
                ApplicableInstructorId = instructorId1, // Instructor-specific
                ApplicableCourseId = null,
                ValidFrom = now,
                ValidTo = now.AddMonths(1),
                IsActive = true,
                CreatedAt = now,
                CreatedBy = instructorId1
            },

            new()
            {
                Id = Guid.CreateVersion7(),
                Code = "MYBRAND50K",
                Description = "Giảm 50,000 VND cho khóa học của tôi",
                Type = CouponType.FixedAmount,
                Value = 50000,
                MinOrderAmount = 400000,
                MaxDiscountAmount = null,
                UsageLimit = 30,
                UsagePerUser = 1,
                UsedCount = 0,
                ApplicableInstructorId = instructorId1, // Instructor-specific
                ApplicableCourseId = null,
                ValidFrom = now,
                ValidTo = now.AddDays(45),
                IsActive = true,
                CreatedAt = now,
                CreatedBy = instructorId1
            }
        };

        await context.Coupons.AddRangeAsync(coupons);
        await context.SaveChangesAsync();
    }

    public static async Task SeedWalletsAsync(SaleDbContext context)
    {
        if (await context.InstructorWallets.AnyAsync())
            return;

        var now = DateTime.UtcNow;

        // IDs from Identity service seed data
        var instructorId1 = new Guid("00000000-0000-0000-0000-000000000006"); // Instructor 1

        var wallets = new List<InstructorWallet>
        {
            new()
            {
                Id = Guid.CreateVersion7(),
                InstructorId = instructorId1,
                AvailableBalance = 50000000, // 50 triệu VND để test
                TotalEarnings = 0,
                TotalWithdrawn = 0,
                Currency = "VND",
                IsActive = true,
                CreatedAt = now
            }
        };

        await context.InstructorWallets.AddRangeAsync(wallets);
        await context.SaveChangesAsync();
    }
}
