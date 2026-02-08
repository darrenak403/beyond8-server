using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Sale.Domain.Enums;
using Beyond8.Common.Data.Base;

namespace Beyond8.Sale.Domain.Entities;

/// <summary>
/// Represents a discount coupon
/// Can be created by Admin (platform-wide) or Instructor (course-specific)
/// </summary>
public class Coupon : BaseEntity
{
    // Coupon Code
    [Required, MaxLength(50)]
    public string Code { get; set; } = string.Empty; 
    [MaxLength(500)]
    public string? Description { get; set; }

    // Coupon Type & Value
    public CouponType Type { get; set; }

    /// <summary>
    /// For Percentage: 0-100 (e.g., 20 = 20% off)
    /// For FixedAmount: Actual amount (e.g., 50000 = 50,000 VND off)
    /// </summary>
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Value { get; set; }

    // Restrictions
    [Column(TypeName = "decimal(18, 2)")]
    public decimal? MinOrderAmount { get; set; } 

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? MaxDiscountAmount { get; set; }
    // Usage Limits
    public int? UsageLimit { get; set; }

    public int? UsagePerUser { get; set; } 

    public int UsedCount { get; set; } = 0;

    // Applicability (null = applies to all)
    /// <summary>
    /// If set, coupon only applies to courses from this instructor
    /// </summary>
    public Guid? ApplicableInstructorId { get; set; }

    /// <summary>
    /// If set, coupon only applies to this specific course
    /// </summary>
    public Guid? ApplicableCourseId { get; set; }

    // Validity Period
    public DateTime ValidFrom { get; set; }

    public DateTime ValidTo { get; set; }

    // Status
    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public virtual ICollection<CouponUsage> CouponUsages { get; set; } = [];
}
