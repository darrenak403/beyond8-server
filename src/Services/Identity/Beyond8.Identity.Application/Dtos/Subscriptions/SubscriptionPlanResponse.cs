namespace Beyond8.Identity.Application.Dtos.Users;

public class SubscriptionPlanResponse
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "VND";
    public int DurationDays { get; set; }
    public int TotalRequestsInPeriod { get; set; }
    public int MaxRequestsPerWeek { get; set; }
    public List<string> Includes { get; set; } = [];
}
