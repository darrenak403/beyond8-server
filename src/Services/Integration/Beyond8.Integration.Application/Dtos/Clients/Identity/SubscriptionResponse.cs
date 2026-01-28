namespace Beyond8.Integration.Application.Dtos.Clients.Identity;

public class SubscriptionResponse
{
    public int RemainingRequests { get; set; }
    public bool IsRequestLimitedReached { get; set; }
    public DateTime? RequestLimitedEndsAt { get; set; }
    public SubscriptionPlanResponse? SubscriptionPlan { get; set; }
}
