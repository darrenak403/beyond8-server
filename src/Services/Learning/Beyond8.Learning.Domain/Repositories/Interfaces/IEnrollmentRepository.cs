using Beyond8.Common.Data.Interfaces;

namespace Beyond8.Learning.Domain.Repositories.Interfaces;

public interface IEnrollmentRepository : IGenericRepository<Entities.Enrollment>
{
    Task<int> CountActiveByCourseIdAsync(Guid courseId);
    Task<List<Guid>> GetEnrolledCourseIdsAsync(Guid userId);
}
