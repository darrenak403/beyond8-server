namespace Beyond8.Sale.Application.Dtos.CouponUsages;

public class CouponUsageResponse
{
    public Guid Id { get; set; }
    public Guid CouponId { get; set; }
    public Guid UserId { get; set; }
    public Guid OrderId { get; set; }

    public string CouponCode { get; set; } = string.Empty;
    public string CouponType { get; set; } = string.Empty;
    public decimal DiscountValue { get; set; }
    public decimal DiscountApplied { get; set; }
    public decimal OrderSubtotal { get; set; }

    public DateTime UsedAt { get; set; }

    public string? UserEmail { get; set; }
    public string? OrderNumber { get; set; }
}
