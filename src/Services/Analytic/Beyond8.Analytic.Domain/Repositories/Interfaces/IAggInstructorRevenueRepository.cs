using Beyond8.Analytic.Domain.Entities;
using Beyond8.Common.Data.Interfaces;

namespace Beyond8.Analytic.Domain.Repositories.Interfaces;

public interface IAggInstructorRevenueRepository : IGenericRepository<AggInstructorRevenue>
{
    Task<AggInstructorRevenue?> GetByInstructorIdAsync(Guid instructorId);
}
