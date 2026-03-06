using Beyond8.Analytic.Domain.Repositories.Interfaces;
using Beyond8.Analytic.Infrastructure.Data;
using Beyond8.Common.Data.Implements;

namespace Beyond8.Analytic.Infrastructure.Repositories.Implements;

public class UnitOfWork(AnalyticDbContext context) : BaseUnitOfWork<AnalyticDbContext>(context), IUnitOfWork
{
    private IAggCourseStatsRepository? _aggCourseStatsRepository;
    private IAggLessonPerformanceRepository? _aggLessonPerformanceRepository;
    private IAggInstructorRevenueRepository? _aggInstructorRevenueRepository;
    private IAggSystemOverviewRepository? _aggSystemOverviewRepository;
    private IAggSystemOverviewMonthlyRepository? _aggSystemOverviewMonthlyRepository;
    private IAggAiUsageDailyRepository? _aggAiUsageDailyRepository;

    public IAggCourseStatsRepository AggCourseStatsRepository =>
        _aggCourseStatsRepository ??= new AggCourseStatsRepository(context);

    public IAggLessonPerformanceRepository AggLessonPerformanceRepository =>
        _aggLessonPerformanceRepository ??= new AggLessonPerformanceRepository(context);

    public IAggInstructorRevenueRepository AggInstructorRevenueRepository =>
        _aggInstructorRevenueRepository ??= new AggInstructorRevenueRepository(context);

    public IAggSystemOverviewRepository AggSystemOverviewRepository =>
        _aggSystemOverviewRepository ??= new AggSystemOverviewRepository(context);

    public IAggSystemOverviewMonthlyRepository AggSystemOverviewMonthlyRepository =>
        _aggSystemOverviewMonthlyRepository ??= new AggSystemOverviewMonthlyRepository(context);

    public IAggAiUsageDailyRepository AggAiUsageDailyRepository =>
        _aggAiUsageDailyRepository ??= new AggAiUsageDailyRepository(context);
}
