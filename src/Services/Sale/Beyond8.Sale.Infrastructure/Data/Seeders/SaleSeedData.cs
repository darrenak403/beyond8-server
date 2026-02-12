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

        var systemId = Guid.Empty;
        var now = DateTime.UtcNow;

        // IDs from Identity service seed data
        var instructorId1 = new Guid("00000000-0000-0000-0000-000000000006"); // Instructor 1
        var instructorId2 = new Guid("00000000-0000-0000-0000-000000000007"); // Instructor 2 (if exists)

        var coupons = new List<Coupon>
        {
            // ═══════════════════════════════════════════════════════════
            // System-Wide Coupons (Admin Created - Platform Coupons)
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
                Code = "NEWYEAR2026",
                Description = "Giảm 30% dịp Tết Nguyên Đán (tối đa 500,000 VND)",
                Type = CouponType.Percentage,
                Value = 30,
                MinOrderAmount = 1000000,
                MaxDiscountAmount = 500000,
                UsageLimit = 200,
                UsagePerUser = 1,
                UsedCount = 0,
                ApplicableInstructorId = null, // System coupon
                ApplicableCourseId = null,
                ValidFrom = now,
                ValidTo = now.AddDays(30),
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

            // ═══════════════════════════════════════════════════════════
            // Instructor-Specific Coupons
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
            },

            // ═══════════════════════════════════════════════════════════
            // Test/Demo Coupons (Inactive or Expired)
            // ═══════════════════════════════════════════════════════════

            new()
            {
                Id = Guid.CreateVersion7(),
                Code = "EXPIRED10",
                Description = "Coupon đã hết hạn - dùng để test",
                Type = CouponType.Percentage,
                Value = 10,
                MinOrderAmount = 100000,
                MaxDiscountAmount = 50000,
                UsageLimit = 100,
                UsagePerUser = 1,
                UsedCount = 0,
                ApplicableInstructorId = null,
                ApplicableCourseId = null,
                ValidFrom = now.AddDays(-30),
                ValidTo = now.AddDays(-1), // Expired
                IsActive = true,
                CreatedAt = now.AddDays(-30),
                CreatedBy = systemId
            },

            new()
            {
                Id = Guid.CreateVersion7(),
                Code = "INACTIVE50",
                Description = "Coupon bị vô hiệu hóa - dùng để test",
                Type = CouponType.Percentage,
                Value = 50,
                MinOrderAmount = 200000,
                MaxDiscountAmount = 100000,
                UsageLimit = 10,
                UsagePerUser = 1,
                UsedCount = 0,
                ApplicableInstructorId = null,
                ApplicableCourseId = null,
                ValidFrom = now,
                ValidTo = now.AddMonths(1),
                IsActive = false, // Inactive
                CreatedAt = now,
                CreatedBy = systemId
            },

            new()
            {
                Id = Guid.CreateVersion7(),
                Code = "LIMITREACHED",
                Description = "Coupon đã hết lượt dùng - dùng để test",
                Type = CouponType.FixedAmount,
                Value = 100000,
                MinOrderAmount = 500000,
                MaxDiscountAmount = null,
                UsageLimit = 5,
                UsagePerUser = 1,
                UsedCount = 5, // Already reached limit
                ApplicableInstructorId = null,
                ApplicableCourseId = null,
                ValidFrom = now,
                ValidTo = now.AddMonths(1),
                IsActive = true,
                CreatedAt = now,
                CreatedBy = systemId
            },

            // ═══════════════════════════════════════════════════════════
            // Special Event Coupons
            // ═══════════════════════════════════════════════════════════

            new()
            {
                Id = Guid.CreateVersion7(),
                Code = "BLACKFRIDAY50",
                Description = "Black Friday - Giảm 50% (tối đa 1 triệu VND)",
                Type = CouponType.Percentage,
                Value = 50,
                MinOrderAmount = 1000000,
                MaxDiscountAmount = 1000000,
                UsageLimit = 100,
                UsagePerUser = 1,
                UsedCount = 0,
                ApplicableInstructorId = null,
                ApplicableCourseId = null,
                ValidFrom = now,
                ValidTo = now.AddDays(7),
                IsActive = true,
                CreatedAt = now,
                CreatedBy = systemId
            },

            new()
            {
                Id = Guid.CreateVersion7(),
                Code = "STUDENT10",
                Description = "Ưu đãi sinh viên - Giảm 10% không giới hạn",
                Type = CouponType.Percentage,
                Value = 10,
                MinOrderAmount = 200000,
                MaxDiscountAmount = 100000,
                UsageLimit = null, // Unlimited
                UsagePerUser = null, // Unlimited per user
                UsedCount = 0,
                ApplicableInstructorId = null,
                ApplicableCourseId = null,
                ValidFrom = now,
                ValidTo = now.AddYears(1),
                IsActive = true,
                CreatedAt = now,
                CreatedBy = systemId
            }
        };

        await context.Coupons.AddRangeAsync(coupons);
        await context.SaveChangesAsync();
    }
}
