using Beyond8.Analytic.Domain.Entities;
using Beyond8.Analytic.Domain.Repositories.Interfaces;
using Beyond8.Analytic.Infrastructure.Data;
using Beyond8.Common.Data.Implements;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Analytic.Infrastructure.Repositories.Implements;

public class AggSystemOverviewMonthlyRepository(AnalyticDbContext context)
    : PostgresRepository<AggSystemOverviewMonthly>(context), IAggSystemOverviewMonthlyRepository
{
    public async Task<AggSystemOverviewMonthly> GetOrCreateForMonthAsync(string yearMonth, int year, int month)
    {
        var existing = await context.AggSystemOverviewMonthlies
            .FirstOrDefaultAsync(e => e.YearMonth == yearMonth);

        if (existing != null) return existing;

        existing = new AggSystemOverviewMonthly
        {
            YearMonth = yearMonth,
            Year = year,
            Month = month
        };
        await context.AggSystemOverviewMonthlies.AddAsync(existing);
        return existing;
    }

    public async Task<List<AggSystemOverviewMonthly>> GetLastNMonthsAsync(int n)
    {
        return await context.AggSystemOverviewMonthlies
            .OrderByDescending(e => e.YearMonth)
            .Take(n)
            .OrderBy(e => e.YearMonth)
            .ToListAsync();
    }

    public async Task<List<AggSystemOverviewMonthly>> GetByYearAsync(int year)
    {
        return await context.AggSystemOverviewMonthlies
            .Where(e => e.Year == year)
            .OrderBy(e => e.Month)
            .ToListAsync();
    }

    public async Task<List<AggSystemOverviewMonthly>> GetByQuarterAsync(int year, int quarter)
    {
        var startMonth = (quarter - 1) * 3 + 1;
        var endMonth = startMonth + 2;

        return await context.AggSystemOverviewMonthlies
            .Where(e => e.Year == year && e.Month >= startMonth && e.Month <= endMonth)
            .OrderBy(e => e.Month)
            .ToListAsync();
    }

    public async Task<List<AggSystemOverviewMonthly>> GetByYearMonthRangeAsync(
        string fromYearMonth, string toYearMonth)
    {
        return await context.AggSystemOverviewMonthlies
            .Where(e => string.Compare(e.YearMonth, fromYearMonth) >= 0
                     && string.Compare(e.YearMonth, toYearMonth) <= 0)
            .OrderBy(e => e.YearMonth)
            .ToListAsync();
    }
}
