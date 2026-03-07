using Beyond8.Analytic.Domain.Entities;
using Beyond8.Analytic.Domain.Repositories.Interfaces;
using Beyond8.Analytic.Infrastructure.Data;
using Beyond8.Common.Data.Implements;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Analytic.Infrastructure.Repositories.Implements;

public class AggAiUsageDailyRepository(AnalyticDbContext context)
    : PostgresRepository<AggAiUsageDaily>(context), IAggAiUsageDailyRepository
{
    public async Task<List<AggAiUsageDaily>> GetBySnapshotDateAsync(DateOnly snapshotDate)
    {
        return await context.AggAiUsageDailies
            .Where(e => e.SnapshotDate == snapshotDate)
            .ToListAsync();
    }

    public async Task<List<AggAiUsageDaily>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate)
    {
        return await context.AggAiUsageDailies
            .Where(e => e.SnapshotDate >= startDate && e.SnapshotDate <= endDate)
            .OrderBy(e => e.SnapshotDate)
            .ThenBy(e => e.Model)
            .ToListAsync();
    }
}
