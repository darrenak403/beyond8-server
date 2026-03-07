using Beyond8.Common.Utilities;
using Beyond8.Identity.Application.Dtos.Users;

namespace Beyond8.Identity.Application.Services.Interfaces;

public interface ISubscriptionService
{
    Task<ApiResponse<SubscriptionResponse>> GetMySubscriptionStatsAsync(Guid userId);
    Task<ApiResponse<List<SubscriptionPlanResponse>>> GetSubscriptionPlansAsync();
    Task<ApiResponse<SubscriptionResponse>> UpdateSubscriptionAsync(Guid userId, UpdateUsageQuotaRequest request);
    Task<ApiResponse<List<SubscriptionResponse>>> GetAllSubscriptionsAsync();
    Task ResetWeeklyRequestsAsync();
    Task ExpireSubscriptionsAsync();
}
