using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Common.Data.Base;

namespace Beyond8.Sale.Domain.Entities;

/// <summary>
/// Tracks coupon usage history
/// Prevents users from exceeding per-user limits
/// </summary>
public class CouponUsage : BaseEntity
{
    // Coupon Reference
    public Guid CouponId { get; set; }

    [ForeignKey(nameof(CouponId))]
    public virtual Coupon Coupon { get; set; } = null!;

    // Order Reference
    public Guid OrderId { get; set; }

    [ForeignKey(nameof(OrderId))]
    public virtual Order Order { get; set; } = null!;

    // User Reference (Logical - No FK)
    public Guid UserId { get; set; }

    // Usage Information
    public DateTime UsedAt { get; set; }

    /// <summary>
    /// Actual discount amount applied to the order
    /// Calculated based on coupon type and order amount
    /// </summary>
    [Column(TypeName = "decimal(18, 2)")]
    public decimal DiscountAmount { get; set; }
}
