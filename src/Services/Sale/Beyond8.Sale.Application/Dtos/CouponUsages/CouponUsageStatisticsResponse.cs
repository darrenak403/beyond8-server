namespace Beyond8.Sale.Application.Dtos.CouponUsages;

public class CouponUsageStatisticsResponse
{
    public Guid CouponId { get; set; }
    public string CouponCode { get; set; } = string.Empty;
    public string? CouponName { get; set; }

    public int TotalUsageCount { get; set; }
    public int UniqueUserCount { get; set; }

    public decimal TotalDiscountGiven { get; set; }
    public decimal AverageDiscountPerUse { get; set; }

    public DateTime? FirstUsedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }

    public List<TopUser> TopUsers { get; set; } = new();
    public List<DailyUsage> UsageByDay { get; set; } = new();
}

public class TopUser
{
    public Guid UserId { get; set; }
    public string? UserEmail { get; set; }
    public int UsageCount { get; set; }
    public decimal TotalDiscount { get; set; }
}

public class DailyUsage
{
    public DateTime Date { get; set; }
    public int UsageCount { get; set; }
    public decimal TotalDiscount { get; set; }
}
