using Beyond8.Analytic.Domain.Entities;
using Beyond8.Analytic.Domain.Repositories.Interfaces;
using Beyond8.Analytic.Infrastructure.Data;
using Beyond8.Common.Data.Implements;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Analytic.Infrastructure.Repositories.Implements;

public class AggSystemOverviewRepository(AnalyticDbContext context)
    : PostgresRepository<AggSystemOverview>(context), IAggSystemOverviewRepository
{
    public async Task<AggSystemOverview?> GetCurrentAsync()
    {
        return await context.AggSystemOverviews
            .FirstOrDefaultAsync(e => e.IsCurrent);
    }

    public async Task<AggSystemOverview> GetOrCreateCurrentAsync()
    {
        var current = await context.AggSystemOverviews
            .FirstOrDefaultAsync(e => e.IsCurrent);

        if (current != null) return current;

        current = new AggSystemOverview { IsCurrent = true };
        await context.AggSystemOverviews.AddAsync(current);
        return current;
    }
}
