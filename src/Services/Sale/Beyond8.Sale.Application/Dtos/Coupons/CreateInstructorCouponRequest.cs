using Beyond8.Sale.Domain.Enums;

namespace Beyond8.Sale.Application.Dtos.Coupons;

public class CreateInstructorCouponRequest
{
    // Basic Info
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Discount Rules
    public CouponType Type { get; set; }
    public decimal Value { get; set; }
    public decimal? MaxDiscountAmount { get; set; }

    // Usage Limits
    public int? UsageLimit { get; set; }
    public int? UsagePerUser { get; set; }

    // Conditions
    public decimal? MinOrderAmount { get; set; }

    // Target Course (Required for instructor coupons)
    public Guid ApplicableCourseId { get; set; }

    // Validity Period
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }

    // Status
    public bool IsActive { get; set; } = true;
}