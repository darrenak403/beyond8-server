namespace Beyond8.Sale.Application.Dtos.CouponUsages;

public class CouponValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }

    public Guid CouponId { get; set; }
    public string CouponCode { get; set; } = string.Empty;
    public string CouponType { get; set; } = string.Empty;

    public decimal DiscountValue { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public decimal CalculatedDiscount { get; set; }

    public bool IsActive { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }

    public int? UsageLimit { get; set; }
    public int CurrentUsageCount { get; set; }
    public int? UsageLimitPerUser { get; set; }
    public int UserUsageCount { get; set; }

    public bool IsApplicableToCart { get; set; }
    public List<string> ApplicabilityErrors { get; set; } = new();
}
