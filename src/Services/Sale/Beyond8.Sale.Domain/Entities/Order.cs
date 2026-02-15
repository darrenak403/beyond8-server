using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Sale.Domain.Enums;
using Beyond8.Common.Data.Base;

namespace Beyond8.Sale.Domain.Entities;

public class Order : BaseEntity
{
    // User Reference (Logical - No FK)
    public Guid UserId { get; set; }

    // Order Identification
    [Required, MaxLength(50)]
    public string OrderNumber { get; set; } = string.Empty;

    // Order Status
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    // Pricing Information
    [Column(TypeName = "decimal(18, 2)")]
    public decimal OriginalSubTotal { get; set; } // Sum of all original course prices

    [Column(TypeName = "decimal(18, 2)")]
    public decimal SubTotal { get; set; } // Subtotal after instructor discounts (before system discount)

    [Column(TypeName = "decimal(18, 2)")]
    public decimal InstructorDiscountAmount { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal SystemDiscountAmount { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal DiscountAmount { get; set; } // Total discount (Instructor + System)

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? TaxAmount { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalAmount { get; set; }

    // Coupon References
    public Guid? InstructorCouponId { get; set; }
    public Guid? SystemCouponId { get; set; }

    [ForeignKey(nameof(SystemCouponId))]
    public virtual Coupon? SystemCoupon { get; set; }

    [MaxLength(10)]
    public string Currency { get; set; } = "VND";

    // Payment Tracking
    public DateTime? PaidAt { get; set; }

    // Audit & Security
    [MaxLength(45)]
    public string? IpAddress { get; set; }

    [MaxLength(500)]
    public string? UserAgent { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    // Payment Details (JSON for flexibility)
    [Column(TypeName = "jsonb")]
    public string? PaymentDetails { get; set; }

    // Navigation Properties
    public virtual ICollection<OrderItem> OrderItems { get; set; } = [];
    public virtual ICollection<Payment> Payments { get; set; } = [];
}
