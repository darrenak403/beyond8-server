using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Common.Data.Base;

namespace Beyond8.Sale.Domain.Entities;

public class CouponUsage : BaseEntity
{
    public Guid CouponId { get; set; }

    [ForeignKey(nameof(CouponId))]
    public virtual Coupon Coupon { get; set; } = null!;

    public Guid OrderId { get; set; }

    [ForeignKey(nameof(OrderId))]
    public virtual Order Order { get; set; } = null!;

    public Guid UserId { get; set; }

    public DateTime UsedAt { get; set; }
}
