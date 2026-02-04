using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Sale.Domain.Enums;
using Beyond8.Common.Data.Base;

namespace Beyond8.Sale.Domain.Entities;

public class Coupon : BaseEntity
{
    [Required, MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    public CouponType Type { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Value { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? MinOrderAmount { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? MaxDiscountAmount { get; set; }

    public int? UsageLimit { get; set; }

    public int UsedCount { get; set; }

    public DateTime ValidFrom { get; set; }

    public DateTime ValidTo { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual ICollection<CouponUsage> CouponUsages { get; set; } = [];
}
