using Beyond8.Common.Utilities;
using Beyond8.Learning.Application.Dtos.CourseReview;

namespace Beyond8.Learning.Application.Services.Interfaces;

public interface ICourseReviewService
{
    Task<ApiResponse<CourseReviewResponse>> CreateCourseReviewAsync(CreateCourseReviewRequest request, Guid userId);

    Task<ApiResponse<List<CourseReviewResponse>>> GetReviewsByCourseIdAsync(GetCourseReviewsRequest request);
}