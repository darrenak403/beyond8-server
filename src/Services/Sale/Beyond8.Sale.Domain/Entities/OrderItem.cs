using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Common.Data.Base;

namespace Beyond8.Sale.Domain.Entities;

public class OrderItem : BaseEntity
{
    // Order Reference
    public Guid OrderId { get; set; }

    [ForeignKey(nameof(OrderId))]
    public virtual Order Order { get; set; } = null!;

    public Guid CourseId { get; set; }

    [Required, MaxLength(500)]
    public string CourseTitle { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? CourseThumbnail { get; set; }

    public Guid InstructorId { get; set; }

    [Required, MaxLength(200)]
    public string InstructorName { get; set; } = string.Empty;

    // Pricing Information
    [Column(TypeName = "decimal(18, 2)")]
    public decimal OriginalPrice { get; set; } // Original course price

    [Column(TypeName = "decimal(18, 2)")]
    public decimal UnitPrice { get; set; } // Price after instructor discount (if any)

    [Column(TypeName = "decimal(5, 2)")]
    public decimal DiscountPercent { get; set; } = 0; // Instructor-applied discount %

    public int Quantity { get; set; } = 1; // Always 1 for courses

    [Column(TypeName = "decimal(18, 2)")]
    public decimal LineTotal { get; set; } // UnitPrice * Quantity (before platform coupon)

    // Revenue Split (Platform Fee - Per BR-19: 30% platform, 70% instructor)
    [Column(TypeName = "decimal(5, 2)")]
    public decimal PlatformFeePercent { get; set; } = 30; // 30% platform commission

    [Column(TypeName = "decimal(18, 2)")]
    public decimal PlatformFeeAmount { get; set; } = 0;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal InstructorEarnings { get; set; } = 0; // 70% - Amount instructor receives immediately after payment

    // Refund Information (TODO: Implement refund logic later)
    // [Column(TypeName = "decimal(18, 2)")]
    // public decimal RefundedAmount { get; set; } = 0;

    // public DateTime? RefundedAt { get; set; }
}
