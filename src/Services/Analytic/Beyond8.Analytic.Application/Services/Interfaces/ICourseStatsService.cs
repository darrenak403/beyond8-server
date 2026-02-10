using Beyond8.Analytic.Application.Dtos.Common;
using Beyond8.Analytic.Application.Dtos.CourseStats;
using Beyond8.Common.Utilities;

namespace Beyond8.Analytic.Application.Services.Interfaces;

public interface ICourseStatsService
{
    Task<ApiResponse<List<CourseStatsResponse>>> GetAllCourseStatsAsync(DateRangeAnalyticRequest request);
    Task<ApiResponse<CourseStatsResponse>> GetCourseStatsByIdAsync(Guid courseId);
    Task<ApiResponse<List<TopCourseResponse>>> GetTopCoursesAsync(int count = 10, string sortBy = "students");
    Task<ApiResponse<List<CourseStatsResponse>>> GetInstructorCourseStatsAsync(Guid instructorId, PaginationRequest pagination);
}
