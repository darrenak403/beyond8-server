using Beyond8.Common.Clients;
using Beyond8.Common.Utilities;
using Beyond8.Sale.Application.Dtos.Courses;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Beyond8.Sale.Application.Clients.Catalog;

public class CatalogClient(
    HttpClient httpClient,
    IHttpContextAccessor httpContextAccessor,
    ILogger<CatalogClient> logger)
    : BaseClient(httpClient, httpContextAccessor), ICatalogClient
{
    public async Task<ApiResponse<CourseDto>> GetCourseByIdAsync(Guid courseId)
    {
        try
        {
            var data = await GetAsync<CourseDto>($"/api/v1/courses/{courseId}");
            return ApiResponse<CourseDto>.SuccessResponse(data, "OK");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get course from Catalog Service: {CourseId}", courseId);
            return ApiResponse<CourseDto>.FailureResponse(ex.Message);
        }
    }
}