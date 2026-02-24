using Beyond8.Common.Clients;
using Beyond8.Common.Utilities;
using Beyond8.Sale.Application.Dtos.Subscriptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beyond8.Sale.Application.Clients.Identity;

public class IdentityClient(
    HttpClient httpClient,
    IHttpContextAccessor httpContextAccessor,
    ILogger<IdentityClient> logger)
    : BaseClient(httpClient, httpContextAccessor), IIdentityClient
{
    public async Task<ApiResponse<List<SubscriptionPlanDto>>> GetSubscriptionPlansAsync()
    {
        try
        {
            var data = await GetAsync<List<SubscriptionPlanDto>>("/api/v1/subscriptions/plans");
            return ApiResponse<List<SubscriptionPlanDto>>.SuccessResponse(data, "OK");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get subscription plans from Identity Service");
            return ApiResponse<List<SubscriptionPlanDto>>.FailureResponse(ex.Message);
        }
    }
}
