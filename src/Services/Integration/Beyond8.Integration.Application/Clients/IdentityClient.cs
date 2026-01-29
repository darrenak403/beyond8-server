using Beyond8.Common.Clients;
using Beyond8.Common.Utilities;
using Beyond8.Integration.Application.Dtos.Clients.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Beyond8.Integration.Application.Clients;

public class IdentityClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<IdentityClient> logger) : BaseClient(httpClient, httpContextAccessor), IIdentityClient
{
    public async Task<ApiResponse<SubscriptionResponse>> GetUserSubscriptionAsync(Guid userId)
    {
        try
        {
            var data = await GetAsync<SubscriptionResponse>($"/api/v1/subscriptions/me");
            return ApiResponse<SubscriptionResponse>.SuccessResponse(data, "OK");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetUserSubscriptionAsync failed for user {UserId}", userId);
            return ApiResponse<SubscriptionResponse>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<SubscriptionResponse>> UpdateUserSubscriptionAsync(Guid userId, UpdateUsageQuotaRequest request)
    {
        try
        {
            var data = await PatchAsync<SubscriptionResponse>($"/api/v1/subscriptions/{userId}", request);
            return ApiResponse<SubscriptionResponse>.SuccessResponse(data, "OK");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "UpdateUserSubscriptionAsync failed for user {UserId}", userId);
            return ApiResponse<SubscriptionResponse>.FailureResponse(ex.Message);
        }
    }
}
