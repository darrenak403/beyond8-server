using Beyond8.Analytic.Domain.Entities;
using Beyond8.Analytic.Domain.Repositories.Interfaces;
using Beyond8.Analytic.Infrastructure.Data;
using Beyond8.Common.Data.Implements;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Analytic.Infrastructure.Repositories.Implements;

public class AggCourseStatsRepository(AnalyticDbContext context)
    : PostgresRepository<AggCourseStats>(context), IAggCourseStatsRepository
{
    public async Task<AggCourseStats?> GetByCourseIdAsync(Guid courseId)
    {
        return await context.AggCourseStats
            .FirstOrDefaultAsync(e => e.CourseId == courseId && e.IsCurrent);
    }

    public async Task<List<AggCourseStats>> GetByInstructorIdAsync(Guid instructorId)
    {
        return await context.AggCourseStats
            .Where(e => e.InstructorId == instructorId && e.IsCurrent)
            .ToListAsync();
    }
}
