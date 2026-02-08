using Beyond8.Analytic.Domain.Entities;
using Beyond8.Common.Data.Interfaces;

namespace Beyond8.Analytic.Domain.Repositories.Interfaces;

public interface IAggLessonPerformanceRepository : IGenericRepository<AggLessonPerformance>
{
    Task<AggLessonPerformance?> GetByLessonIdAsync(Guid lessonId);
    Task<List<AggLessonPerformance>> GetByCourseIdAsync(Guid courseId);
}
