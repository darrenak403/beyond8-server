using Beyond8.Identity.Application.Dtos.Users;
using Beyond8.Identity.Domain.Entities;

namespace Beyond8.Identity.Application.Mappings.SubscriptionMappings;

public static class SubscriptionMappings
{
    public static SubscriptionResponse ToSubscriptionResponse(this UserSubscription subscription)
    {
        return new SubscriptionResponse
        {
            RemainingRequests = subscription.RemainingRequestsPerWeek,
            IsRequestLimitedReached = IsRequestLimitedReached(subscription),
            RequestLimitedEndsAt = subscription.RequestLimitedEndsAt,
            TotalRemainingRequests = subscription.TotalRemainingRequests,
            ExpiresAt = subscription.ExpiresAt,
            Status = subscription.Status,
            SubscriptionPlan = subscription.Plan != null ? subscription.Plan.ToSubscriptionPlanResponse() : null
        };
    }

    public static bool IsRequestLimitedReached(this UserSubscription subscription)
    {
        var now = DateTime.UtcNow;

        if (subscription.RemainingRequestsPerWeek <= 0) return true;

        if (subscription.RequestLimitedEndsAt.HasValue && subscription.RequestLimitedEndsAt > now)
            return true;

        return false;
    }

    public static SubscriptionPlanResponse ToSubscriptionPlanResponse(this SubscriptionPlan plan)
    {
        return new SubscriptionPlanResponse
        {
            Code = plan.Code,
            Name = plan.Name,
            Description = plan.Description,
            Price = plan.Price,
            Currency = plan.Currency,
            DurationDays = plan.DurationDays,
            TotalRequestsInPeriod = plan.TotalRequestsInPeriod,
            MaxRequestsPerWeek = plan.MaxRequestsPerWeek,
            Includes = plan.Includes?.ToList() ?? []
        };
    }

    public static void UpdateUsageQuotaRequest(this UserSubscription subscription, UpdateUsageQuotaRequest request)
    {
        if (request.NumberOfRequests <= 0) return;

        subscription.TotalRemainingRequests -= request.NumberOfRequests;
        subscription.RemainingRequestsPerWeek -= request.NumberOfRequests;

        if (subscription.RemainingRequestsPerWeek <= 0)
        {
            subscription.RemainingRequestsPerWeek = 0;

            if (subscription.RequestLimitedEndsAt == null)
            {
                subscription.RequestLimitedEndsAt = GetNextMondayUtc();
            }
        }
    }

    private static DateTime GetNextMondayUtc()
    {
        var today = DateTime.UtcNow;
        int daysUntilMonday = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
        if (daysUntilMonday == 0) daysUntilMonday = 7;

        return today.AddDays(daysUntilMonday).Date;
    }
}
