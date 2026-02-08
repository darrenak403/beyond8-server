using Beyond8.Sale.Domain.Enums;

namespace Beyond8.Sale.Application.Dtos.Coupons;

public class CouponResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CouponType Type { get; set; }
    public decimal Value { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public int? UsageLimit { get; set; }
    public int? UsagePerUser { get; set; }
    public int UsedCount { get; set; }
    public Guid? ApplicableInstructorId { get; set; }
    public Guid? ApplicableCourseId { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}