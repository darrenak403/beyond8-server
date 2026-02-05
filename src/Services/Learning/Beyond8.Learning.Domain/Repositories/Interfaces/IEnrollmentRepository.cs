using Beyond8.Common.Data.Interfaces;
using Beyond8.Learning.Domain.Entities;

namespace Beyond8.Learning.Domain.Repositories.Interfaces;

public interface IEnrollmentRepository : IGenericRepository<Entities.Enrollment>
{
    Task<int> CountActiveByCourseIdAsync(Guid courseId);
    Task<List<Guid>> GetEnrolledCourseIdsAsync(Guid userId);
    Task<List<Enrollment>> GetEnrolledCoursesAsync(Guid userId);
}
