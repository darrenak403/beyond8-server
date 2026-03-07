using Beyond8.Analytic.Domain.Entities;
using Beyond8.Common.Data.Interfaces;

namespace Beyond8.Analytic.Domain.Repositories.Interfaces;

public interface IAggSystemOverviewDailyRepository : IGenericRepository<AggSystemOverviewDaily>
{
    Task<AggSystemOverviewDaily> GetOrCreateForDateAsync(string dateKey, int year, int month, int day);
    Task<List<AggSystemOverviewDaily>> GetByMonthAsync(int year, int month);
    Task<List<AggSystemOverviewDaily>> GetByDateRangeAsync(string fromDateKey, string toDateKey);
}
