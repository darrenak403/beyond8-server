using Beyond8.Analytic.Domain.Entities;
using Beyond8.Analytic.Domain.Repositories.Interfaces;
using Beyond8.Analytic.Infrastructure.Data;
using Beyond8.Common.Data.Implements;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Analytic.Infrastructure.Repositories.Implements;

public class AggInstructorRevenueRepository(AnalyticDbContext context)
    : PostgresRepository<AggInstructorRevenue>(context), IAggInstructorRevenueRepository
{
    public async Task<AggInstructorRevenue?> GetByInstructorIdAsync(Guid instructorId)
    {
        return await context.AggInstructorRevenues
            .FirstOrDefaultAsync(e => e.InstructorId == instructorId && e.IsCurrent);
    }
}
