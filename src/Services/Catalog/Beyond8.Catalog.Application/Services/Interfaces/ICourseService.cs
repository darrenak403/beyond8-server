using Beyond8.Catalog.Application.Dtos.Courses;
using Beyond8.Common.Utilities;

namespace Beyond8.Catalog.Application.Services.Interfaces;

public interface ICourseService
{
    // Tạo & Quản Lý Khóa Học // luồng 2
    Task<ApiResponse<CourseResponse>> CreateCourseAsync(CreateCourseRequest request, Guid currentUserId);
    Task<ApiResponse<CourseResponse>> UpdateCourseMetadataAsync(Guid id, Guid currentUserId, UpdateCourseMetadataRequest request);
    Task<ApiResponse<CourseResponse>> GetCourseByIdAsync(Guid id, Guid currentUserId);
    Task<ApiResponse<bool>> DeleteCourseAsync(Guid id, Guid currentUserId);

    // Instructor-Specific Operations // luồng 2
    Task<ApiResponse<List<CourseResponse>>> GetCoursesByInstructorAsync(Guid instructorId, PaginationRequest pagination);
    Task<ApiResponse<CourseStatsDto>> GetCourseStatsByInstructorAsync(Guid instructorId);

    // Phê Duyệt Khóa Học
    Task<ApiResponse<bool>> SubmitForApprovalAsync(Guid courseId, Guid currentUserId);
    Task<ApiResponse<bool>> ApproveCourseAsync(Guid courseId, ApproveCourseRequest request);
    Task<ApiResponse<bool>> RejectCourseAsync(Guid courseId, RejectCourseRequest request);
    Task<ApiResponse<List<CourseResponse>>> GetPendingApprovalCoursesAsync(PaginationRequest pagination);

    // Publishing & Public Access // luồng 2
    Task<ApiResponse<bool>> PublishCourseAsync(Guid courseId, Guid currentUserId);
    Task<ApiResponse<bool>> UnpublishCourseAsync(Guid courseId, Guid currentUserId);
    Task<ApiResponse<List<CourseResponse>>> GetAllCoursesAsync(PaginationCourseSearchRequest request);

    // Đăng Ký Học // luồng 3 
    // Task<ApiResponse<CourseDetailResponse>> GetCourseDetailForEnrollmentAsync(Guid courseId);
    // Task<ApiResponse<bool>> CheckCourseAccessAsync(Guid courseId, Guid userId);
    // Task<ApiResponse<List<CourseResponse>>> GetEnrolledCoursesAsync(Guid userId, PaginationRequest pagination);

    // Analytics & Reporting (Luồng 5) 
    // Task<ApiResponse<CourseAnalyticsDto>> GetCourseAnalyticsAsync(Guid courseId);
    // Task<ApiResponse<List<CourseResponse>>> GetTopRatedCoursesAsync(int topCount = 10);
    // Task<ApiResponse<List<CourseResponse>>> GetMostPopularCoursesAsync(int topCount = 10);

    // Utility Operations // luồng 2
    // Task<ApiResponse<List<CourseResponse>>> SearchCoursesAsync(string searchTerm, PaginationRequest pagination);
}