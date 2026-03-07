using Beyond8.Analytic.Domain.Entities;
using Beyond8.Common.Data.Interfaces;

namespace Beyond8.Analytic.Domain.Repositories.Interfaces;

public interface IAggAiUsageDailyRepository : IGenericRepository<AggAiUsageDaily>
{
    Task<List<AggAiUsageDaily>> GetBySnapshotDateAsync(DateOnly snapshotDate);
    Task<List<AggAiUsageDaily>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate);
}
