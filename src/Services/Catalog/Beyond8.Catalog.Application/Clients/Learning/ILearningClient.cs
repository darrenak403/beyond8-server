using Beyond8.Common.Utilities;

namespace Beyond8.Catalog.Application.Clients.Learning
{
    public interface ILearningClient
    {
        Task<ApiResponse<bool>> IsUserEnrolledInCourseAsync(Guid courseId);
    }
}