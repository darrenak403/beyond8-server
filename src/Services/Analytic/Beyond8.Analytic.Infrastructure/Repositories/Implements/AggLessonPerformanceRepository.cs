using Beyond8.Analytic.Domain.Entities;
using Beyond8.Analytic.Domain.Repositories.Interfaces;
using Beyond8.Analytic.Infrastructure.Data;
using Beyond8.Common.Data.Implements;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Analytic.Infrastructure.Repositories.Implements;

public class AggLessonPerformanceRepository(AnalyticDbContext context)
    : PostgresRepository<AggLessonPerformance>(context), IAggLessonPerformanceRepository
{
    public async Task<AggLessonPerformance?> GetByLessonIdAsync(Guid lessonId)
    {
        return await context.AggLessonPerformances
            .FirstOrDefaultAsync(e => e.LessonId == lessonId && e.IsCurrent);
    }

    public async Task<List<AggLessonPerformance>> GetByCourseIdAsync(Guid courseId)
    {
        return await context.AggLessonPerformances
            .Where(e => e.CourseId == courseId && e.IsCurrent)
            .OrderBy(e => e.CreatedAt)
            .ToListAsync();
    }
}
