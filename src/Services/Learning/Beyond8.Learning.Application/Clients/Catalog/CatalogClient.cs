using Beyond8.Common.Clients;
using Beyond8.Common.Utilities;
using Beyond8.Learning.Application.Dtos.Catalog;
using Microsoft.AspNetCore.Http;

namespace Beyond8.Learning.Application.Clients.Catalog;

public class CatalogClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor) : BaseClient(httpClient, httpContextAccessor), ICatalogClient
{
    public async Task<ApiResponse<CourseStructureResponse>> GetCourseStructureAsync(Guid courseId)
    {
        try
        {
            var data = await GetAsync<CourseStructureResponse>($"/api/v1/courses/{courseId}/summary");
            return ApiResponse<CourseStructureResponse>.SuccessResponse(data, "Lấy cấu trúc khóa học thành công.");
        }
        catch (Exception ex)
        {
            return ApiResponse<CourseStructureResponse>.FailureResponse(ex.Message);
        }
    }
}
