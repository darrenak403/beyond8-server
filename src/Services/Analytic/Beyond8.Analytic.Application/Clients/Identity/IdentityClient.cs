using Beyond8.Analytic.Application.Dtos.Stats;
using Beyond8.Common.Clients;
using Beyond8.Common.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Beyond8.Analytic.Application.Clients.Identity;

public class IdentityClient(
    HttpClient httpClient,
    IHttpContextAccessor httpContextAccessor,
    ILogger<IdentityClient> logger)
    : BaseClient(httpClient, httpContextAccessor), IIdentityClient
{
    public async Task<ApiResponse<PlatformUserStatsResponse>> GetPlatformUserStatsAsync()
    {
        try
        {
            var data = await GetAsync<PlatformUserStatsResponse>("/api/v1/internal/stats/users");
            return ApiResponse<PlatformUserStatsResponse>.SuccessResponse(data, "OK");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get platform user stats from Identity Service");
            return ApiResponse<PlatformUserStatsResponse>.FailureResponse(ex.Message);
        }
    }
}
