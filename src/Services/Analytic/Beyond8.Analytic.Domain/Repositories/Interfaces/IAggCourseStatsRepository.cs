using Beyond8.Analytic.Domain.Entities;
using Beyond8.Common.Data.Interfaces;

namespace Beyond8.Analytic.Domain.Repositories.Interfaces;

public interface IAggCourseStatsRepository : IGenericRepository<AggCourseStats>
{
    Task<AggCourseStats?> GetByCourseIdAsync(Guid courseId);
    Task<List<AggCourseStats>> GetByInstructorIdAsync(Guid instructorId);
}
