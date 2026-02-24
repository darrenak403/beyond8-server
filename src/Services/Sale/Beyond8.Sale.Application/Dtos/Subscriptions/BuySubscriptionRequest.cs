using System;

namespace Beyond8.Sale.Application.Dtos.Subscriptions;

public class BuySubscriptionRequest
{
    // Subscription plan code from Identity service (e.g. "FREE", "PLUS", "PRO")
    public string PlanCode { get; set; } = null!;
}
