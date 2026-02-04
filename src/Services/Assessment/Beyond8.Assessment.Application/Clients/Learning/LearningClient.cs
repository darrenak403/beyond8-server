using Beyond8.Common.Clients;
using Beyond8.Common.Utilities;
using Microsoft.AspNetCore.Http;

namespace Beyond8.Assessment.Application.Clients.Learning;

public class LearningClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    : BaseClient(httpClient, httpContextAccessor), ILearningClient
{
    public async Task<ApiResponse<bool>> IsUserEnrolledInCourseAsync(Guid courseId)
    {
        try
        {
            var isEnrolled = await GetAsync<bool>($"/api/v1/enrollments/check?courseId={courseId}");
            return ApiResponse<bool>.SuccessResponse(isEnrolled, isEnrolled ? "Đã đăng ký khóa học." : "Chưa đăng ký khóa học.");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.FailureResponse(ex.Message);
        }
    }
}
