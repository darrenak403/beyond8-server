using Beyond8.Analytic.Domain.Entities;
using Beyond8.Common.Data.Interfaces;

namespace Beyond8.Analytic.Domain.Repositories.Interfaces;

public interface IAggSystemOverviewRepository : IGenericRepository<AggSystemOverview>
{
    Task<AggSystemOverview?> GetCurrentAsync();
    Task<AggSystemOverview> GetOrCreateCurrentAsync();
}
