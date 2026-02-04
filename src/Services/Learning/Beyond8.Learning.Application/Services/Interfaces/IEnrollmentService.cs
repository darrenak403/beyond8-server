using Beyond8.Common.Utilities;
using Beyond8.Learning.Application.Dtos.Enrollments;

namespace Beyond8.Learning.Application.Services.Interfaces;

public interface IEnrollmentService
{
    Task<ApiResponse<EnrollmentResponse>> EnrollFreeAsync(Guid userId, EnrollFreeRequest request);
    Task<ApiResponse<bool>> IsUserEnrolledInCourseAsync(Guid userId, Guid courseId);
}
