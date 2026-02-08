using Beyond8.Sale.Application.Dtos.Coupons;
using Beyond8.Sale.Domain.Entities;

namespace Beyond8.Sale.Application.Mappings.Coupons;

public static class CouponMappings
{
    public static CouponResponse ToResponse(this Coupon coupon)
    {
        return new CouponResponse
        {
            Id = coupon.Id,
            Code = coupon.Code,
            Description = coupon.Description,
            Type = coupon.Type,
            Value = coupon.Value,
            MinOrderAmount = coupon.MinOrderAmount,
            MaxDiscountAmount = coupon.MaxDiscountAmount,
            UsageLimit = coupon.UsageLimit,
            UsagePerUser = coupon.UsagePerUser,
            UsedCount = coupon.UsedCount,
            ApplicableInstructorId = coupon.ApplicableInstructorId,
            ApplicableCourseId = coupon.ApplicableCourseId,
            ValidFrom = coupon.ValidFrom,
            ValidTo = coupon.ValidTo,
            IsActive = coupon.IsActive,
            CreatedAt = coupon.CreatedAt,
            UpdatedAt = coupon.UpdatedAt
        };
    }

    public static Coupon ToEntity(this CreateCouponRequest request)
    {
        return new Coupon
        {
            Code = request.Code.ToUpper(), // Normalize to uppercase
            Description = request.Description,
            Type = request.Type,
            Value = request.Value,
            MinOrderAmount = request.MinOrderAmount,
            MaxDiscountAmount = request.MaxDiscountAmount,
            UsageLimit = request.UsageLimit,
            UsagePerUser = request.UsagePerUser,
            ApplicableInstructorId = request.ApplicableInstructorId,
            ApplicableCourseId = request.ApplicableCourseId,
            ValidFrom = request.ValidFrom,
            ValidTo = request.ValidTo,
            IsActive = request.IsActive
        };
    }

    public static void UpdateFrom(this Coupon coupon, UpdateCouponRequest request)
    {
        if (request.Description != null)
            coupon.Description = request.Description;

        if (request.Type.HasValue)
            coupon.Type = request.Type.Value;

        if (request.Value.HasValue)
            coupon.Value = request.Value.Value;

        if (request.MinOrderAmount.HasValue)
            coupon.MinOrderAmount = request.MinOrderAmount;

        if (request.MaxDiscountAmount.HasValue)
            coupon.MaxDiscountAmount = request.MaxDiscountAmount;

        if (request.UsageLimit.HasValue)
            coupon.UsageLimit = request.UsageLimit;

        if (request.UsagePerUser.HasValue)
            coupon.UsagePerUser = request.UsagePerUser;

        if (request.ApplicableInstructorId.HasValue)
            coupon.ApplicableInstructorId = request.ApplicableInstructorId;

        if (request.ApplicableCourseId.HasValue)
            coupon.ApplicableCourseId = request.ApplicableCourseId;

        if (request.ValidFrom.HasValue)
            coupon.ValidFrom = request.ValidFrom.Value;

        if (request.ValidTo.HasValue)
            coupon.ValidTo = request.ValidTo.Value;

        if (request.IsActive.HasValue)
            coupon.IsActive = request.IsActive.Value;

        coupon.UpdatedAt = DateTime.UtcNow;
    }
}