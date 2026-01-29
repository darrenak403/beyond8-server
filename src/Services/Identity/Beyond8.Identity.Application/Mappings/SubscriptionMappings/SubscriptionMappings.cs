using System;
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
            IsRequestLimitedReached = subscription.RemainingRequestsPerWeek <= 0,
            RequestLimitedEndsAt = subscription.RequestLimitedEndsAt,
            SubscriptionPlan = subscription.Plan != null ? subscription.Plan.ToSubscriptionPlanResponse() : null
        };
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
        if (request.NumberOfRequests > 0)
        {
            subscription.TotalRemainingRequests -= request.NumberOfRequests;
            subscription.RemainingRequestsPerWeek -= request.NumberOfRequests;
            if (subscription.RemainingRequestsPerWeek < 0)
            {
                subscription.RemainingRequestsPerWeek = 0;
                subscription.RequestLimitedEndsAt = DateTime.UtcNow.AddDays(7);
            }
            else
            {
                subscription.RequestLimitedEndsAt = null;
            }
        }
    }
}
