using Beyond8.Catalog.Application.Dtos.Courses;
using Beyond8.Common.Utilities;

namespace Beyond8.Catalog.Application.Services.Interfaces;

public interface ICourseService
{
    Task<ApiResponse<CourseResponse>> CreateCourseAsync(CreateCourseRequest request, Guid currentUserId);
    Task<ApiResponse<CourseResponse>> UpdateCourseMetadataAsync(Guid id, Guid currentUserId, UpdateCourseMetadataRequest request);
    Task<ApiResponse<CourseResponse>> GetCourseByIdAsync(Guid id, Guid currentUserId);
    Task<ApiResponse<bool>> DeleteCourseAsync(Guid id, Guid currentUserId);
    Task<ApiResponse<bool>> UpdateCourseThumbnailAsync(Guid courseId, Guid currentUserId, UpdateCourseThumbnailRequest request);
    Task<ApiResponse<CourseResponse>> SetCourseDiscountAsync(Guid courseId, Guid currentUserId, SetCourseDiscountRequest request);
    Task<ApiResponse<List<CourseResponse>>> GetCoursesByInstructorAsync(Guid instructorId, PaginationCourseInstructorSearchRequest pagination);
    Task<ApiResponse<CourseStatsResponse>> GetCourseStatsByInstructorAsync(Guid instructorId);
    Task<ApiResponse<bool>> SubmitForApprovalAsync(Guid courseId, Guid currentUserId);
    Task<ApiResponse<bool>> ApproveCourseAsync(Guid courseId, ApproveCourseRequest request);
    Task<ApiResponse<bool>> RejectCourseAsync(Guid courseId, RejectCourseRequest request);
    Task<ApiResponse<List<CourseResponse>>> GetAllCoursesForAdminAsync(PaginationCourseAdminSearchRequest pagination);
    Task<ApiResponse<bool>> PublishCourseAsync(Guid courseId, Guid currentUserId);
    Task<ApiResponse<bool>> UnpublishCourseAsync(Guid courseId, Guid currentUserId);
    Task<ApiResponse<List<CourseSimpleResponse>>> GetAllCoursesAsync(PaginationCourseSearchRequest request);
    Task<ApiResponse<List<CourseResponse>>> FullTextSearchCoursesAsync(FullTextSearchRequest request);
    Task<ApiResponse<CourseSummaryResponse>> GetCourseSummaryAsync(Guid courseId);
    Task<ApiResponse<CourseDetailResponse>> GetCourseDetailsAsync(Guid courseId, Guid userId);
    Task<ApiResponse<CourseDetailResponse>> GetCourseDetailsForAdminAsync(Guid courseId);
    Task<ApiResponse<List<CourseResponse>>> GetCoursesByInstructorIdAsync(Guid instructorId, PaginationRequest pagination);
    Task<ApiResponse<List<CourseResponse>>> GetMostPopularCoursesAsync(PaginationCourseSearchRequest pagination);
}