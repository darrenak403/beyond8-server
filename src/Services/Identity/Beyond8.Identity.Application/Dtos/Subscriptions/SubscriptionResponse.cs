using Beyond8.Identity.Domain.Enums;

namespace Beyond8.Identity.Application.Dtos.Users;

public class SubscriptionResponse
{
    public int RemainingRequests { get; set; }
    public bool IsRequestLimitedReached { get; set; }
    public DateTime? RequestLimitedEndsAt { get; set; }
    public SubscriptionPlanResponse? SubscriptionPlan { get; set; }
    public int TotalRemainingRequests { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public SubscriptionStatus Status { get; set; }
}
