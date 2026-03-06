using Beyond8.Analytic.Domain.Entities;
using Beyond8.Analytic.Domain.Repositories.Interfaces;
using Beyond8.Analytic.Infrastructure.Data;
using Beyond8.Common.Data.Implements;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Analytic.Infrastructure.Repositories.Implements;

public class AggSystemOverviewDailyRepository(AnalyticDbContext context)
    : PostgresRepository<AggSystemOverviewDaily>(context), IAggSystemOverviewDailyRepository
{
    public async Task<AggSystemOverviewDaily> GetOrCreateForDateAsync(
        string dateKey, int year, int month, int day)
    {
        var existing = await context.AggSystemOverviewDailies
            .FirstOrDefaultAsync(e => e.DateKey == dateKey);

        if (existing != null) return existing;

        existing = new AggSystemOverviewDaily
        {
            DateKey = dateKey,
            Year = year,
            Month = month,
            Day = day
        };
        await context.AggSystemOverviewDailies.AddAsync(existing);
        return existing;
    }

    public async Task<List<AggSystemOverviewDaily>> GetByMonthAsync(int year, int month)
    {
        return await context.AggSystemOverviewDailies
            .Where(e => e.Year == year && e.Month == month)
            .OrderBy(e => e.Day)
            .ToListAsync();
    }

    public async Task<List<AggSystemOverviewDaily>> GetByDateRangeAsync(
        string fromDateKey, string toDateKey)
    {
        return await context.AggSystemOverviewDailies
            .Where(e => string.Compare(e.DateKey, fromDateKey) >= 0
                     && string.Compare(e.DateKey, toDateKey) <= 0)
            .OrderBy(e => e.DateKey)
            .ToListAsync();
    }
}
