using Beyond8.Analytic.Domain.Entities;
using Beyond8.Common.Data.Interfaces;

namespace Beyond8.Analytic.Domain.Repositories.Interfaces;

public interface IAggSystemOverviewMonthlyRepository : IGenericRepository<AggSystemOverviewMonthly>
{
    Task<AggSystemOverviewMonthly> GetOrCreateForMonthAsync(string yearMonth, int year, int month);
    Task<List<AggSystemOverviewMonthly>> GetLastNMonthsAsync(int n);
}
