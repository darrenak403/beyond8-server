using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Sale.Domain.Enums;
using Beyond8.Common.Data.Base;

namespace Beyond8.Sale.Domain.Entities;

public class Order : BaseEntity
{
    public Guid UserId { get; set; }

    [Required, MaxLength(50)]
    public string OrderNumber { get; set; } = string.Empty;

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal SubTotal { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal DiscountAmount { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? TaxAmount { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalAmount { get; set; }

    public Guid? CouponId { get; set; }

    [ForeignKey(nameof(CouponId))]
    public virtual Coupon? Coupon { get; set; }

    [MaxLength(10)]
    public string Currency { get; set; } = "VND";

    public virtual ICollection<OrderItem> OrderItems { get; set; } = [];
    public virtual ICollection<Payment> Payments { get; set; } = [];
}
