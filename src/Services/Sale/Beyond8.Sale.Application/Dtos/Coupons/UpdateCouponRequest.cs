namespace Beyond8.Sale.Application.Dtos.Coupons;

public class UpdateCouponRequest
{
    public string? Type { get; set; }
    public decimal? Value { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public int? UsageLimit { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool? IsActive { get; set; }
}