using Beyond8.Sale.Application.Dtos.CouponUsages;
using Beyond8.Sale.Domain.Entities;

namespace Beyond8.Sale.Application.Mappings.CouponUsages;

public static class CouponUsageMappings
{
    public static CouponUsageResponse ToResponse(this CouponUsage usage)
    {
        return new CouponUsageResponse
        {
            Id = usage.Id,
            CouponId = usage.CouponId,
            UserId = usage.UserId,
            OrderId = usage.OrderId,
            CouponCode = usage.Coupon?.Code ?? string.Empty,
            CouponType = usage.Coupon?.Type.ToString() ?? string.Empty,
            DiscountValue = usage.Coupon?.Value ?? 0,
            DiscountApplied = usage.DiscountAmount,
            OrderSubtotal = usage.Order?.TotalAmount ?? 0,
            UsedAt = usage.UsedAt,
            OrderNumber = usage.Order?.OrderNumber
        };
    }

    public static CouponUsage ToEntity(this CreateCouponUsageRequest request)
    {
        return new CouponUsage
        {
            CouponId = request.CouponId,
            UserId = request.UserId,
            OrderId = request.OrderId,
            UsedAt = DateTime.UtcNow,
            DiscountAmount = request.DiscountApplied
        };
    }
}