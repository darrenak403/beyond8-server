using Beyond8.Common.Utilities;
using Beyond8.Learning.Application.Dtos.Enrollments;

namespace Beyond8.Learning.Application.Services.Interfaces;

public interface IEnrollmentService
{
    Task<ApiResponse<EnrollmentResponse>> EnrollFreeAsync(Guid userId, EnrollFreeRequest request);
    Task<ApiResponse<bool>> EnrollPaidCoursesAsync(Guid userId, List<Guid> courseIds, Guid orderId);
    Task<ApiResponse<bool>> IsUserEnrolledInCourseAsync(Guid userId, Guid courseId);
    Task<ApiResponse<List<Guid>>> GetEnrolledCourseIdsAsync(Guid userId);
    Task<ApiResponse<List<EnrollmentSimpleResponse>>> GetEnrolledCoursesAsync(Guid userId);
    Task<ApiResponse<EnrollmentResponse>> GetEnrollmentByIdAsync(Guid id, Guid userId);
    Task<ApiResponse<bool>> HasCertificateForCourseAsync(Guid studentId, Guid courseId);
}
