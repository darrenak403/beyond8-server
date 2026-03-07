using Beyond8.Common.Clients;
using Beyond8.Common.Utilities;

namespace Beyond8.Assessment.Application.Clients.Learning;


public interface ILearningClient : IBaseClient
{
    Task<ApiResponse<bool>> IsUserEnrolledInCourseAsync(Guid courseId);
    Task<ApiResponse<bool>> HasCertificateForCourseAsync(Guid courseId, Guid studentId);
}
