using Beyond8.Analytic.Application.Dtos.Stats;
using Beyond8.Common.Clients;
using Beyond8.Common.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Beyond8.Analytic.Application.Clients.Learning;

public class LearningClient(
    HttpClient httpClient,
    IHttpContextAccessor httpContextAccessor,
    ILogger<LearningClient> logger)
    : BaseClient(httpClient, httpContextAccessor), ILearningClient
{
    public async Task<ApiResponse<PlatformEnrollmentStatsResponse>> GetPlatformEnrollmentStatsAsync()
    {
        try
        {
            var data = await GetAsync<PlatformEnrollmentStatsResponse>("/api/v1/internal/stats/enrollments");
            return ApiResponse<PlatformEnrollmentStatsResponse>.SuccessResponse(data, "OK");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get platform enrollment stats from Learning Service");
            return ApiResponse<PlatformEnrollmentStatsResponse>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<InstructorEnrollmentStatsResponse>> GetInstructorEnrollmentStatsAsync(Guid instructorId)
    {
        try
        {
            var data = await GetAsync<InstructorEnrollmentStatsResponse>(
                $"/api/v1/internal/stats/instructors/{instructorId}/enrollments");
            return ApiResponse<InstructorEnrollmentStatsResponse>.SuccessResponse(data, "OK");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get instructor enrollment stats. InstructorId={InstructorId}", instructorId);
            return ApiResponse<InstructorEnrollmentStatsResponse>.FailureResponse(ex.Message);
        }
    }
}
