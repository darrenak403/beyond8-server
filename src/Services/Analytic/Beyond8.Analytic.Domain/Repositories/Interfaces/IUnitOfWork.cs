using Beyond8.Common.Data.Interfaces;

namespace Beyond8.Analytic.Domain.Repositories.Interfaces;

public interface IUnitOfWork : IBaseUnitOfWork
{
    IAggCourseStatsRepository AggCourseStatsRepository { get; }
    IAggLessonPerformanceRepository AggLessonPerformanceRepository { get; }
    IAggInstructorRevenueRepository AggInstructorRevenueRepository { get; }
    IAggSystemOverviewRepository AggSystemOverviewRepository { get; }
}
