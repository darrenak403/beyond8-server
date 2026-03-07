using Beyond8.Analytic.Application.Dtos.Stats;
using Beyond8.Common.Clients;
using Beyond8.Common.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Beyond8.Analytic.Application.Clients.Catalog;

public class CatalogClient(
    HttpClient httpClient,
    IHttpContextAccessor httpContextAccessor,
    ILogger<CatalogClient> logger)
    : BaseClient(httpClient, httpContextAccessor), ICatalogClient
{
    public async Task<ApiResponse<PlatformCourseStatsResponse>> GetPlatformCourseStatsAsync()
    {
        try
        {
            var data = await GetAsync<PlatformCourseStatsResponse>("/api/v1/internal/stats/courses");
            return ApiResponse<PlatformCourseStatsResponse>.SuccessResponse(data, "OK");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get platform course stats from Catalog Service");
            return ApiResponse<PlatformCourseStatsResponse>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<InstructorCourseStatsResponse>> GetInstructorCourseStatsAsync(Guid instructorId)
    {
        try
        {
            var data = await GetAsync<InstructorCourseStatsResponse>(
                $"/api/v1/internal/stats/instructors/{instructorId}/courses");
            return ApiResponse<InstructorCourseStatsResponse>.SuccessResponse(data, "OK");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get instructor course stats from Catalog Service. InstructorId={InstructorId}", instructorId);
            return ApiResponse<InstructorCourseStatsResponse>.FailureResponse(ex.Message);
        }
    }
}
