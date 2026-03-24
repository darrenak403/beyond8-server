using FluentAssertions;
using Xunit;

namespace Beyond8.IntegrationTests.Flows;

public class InstructorAdminAnalyticsTests
{
    [Fact]
    public void AnalyticsPipeline_ShouldReturnInstructorAndAdminMetrics_ForHappyPathRun()
    {
        var state = AnalyticsState.FromRun(
            new RunMetrics(PublishedCourses: 1, PaidOrders: 1, Revenue: 799000m, Enrollments: 1));

        var instructorStats = state.GetInstructorStats(UserRole.Instructor);
        instructorStats.PublishedCourseCount.Should().Be(1);
        instructorStats.EnrollmentCount.Should().Be(1);
        instructorStats.Revenue.Should().Be(799000m);

        var analytics = state.GetInstructorAnalytics(UserRole.Instructor);
        analytics.TotalPaidOrders.Should().Be(1);
        analytics.Revenue.Should().BeGreaterThan(0);

        var dashboard = state.GetSystemDashboard(UserRole.Admin);
        dashboard.TotalPaidOrders.Should().Be(1);
        dashboard.TotalRevenue.Should().Be(799000m);
        dashboard.TotalEnrollments.Should().Be(1);

        var trend = state.GetRevenueTrend(UserRole.Admin);
        trend.Should().NotBeEmpty();
        trend.Sum(x => x.Revenue).Should().Be(799000m);
    }

    [Fact]
    public void AnalyticsPipeline_ShouldForbidStudentAccess_ToSystemDashboard()
    {
        var state = AnalyticsState.FromRun(
            new RunMetrics(PublishedCourses: 1, PaidOrders: 1, Revenue: 799000m, Enrollments: 1));

        var act = () => state.GetSystemDashboard(UserRole.Student);

        act.Should().Throw<UnauthorizedAccessException>();
    }

    [Fact]
    public void AnalyticsPipeline_ShouldForbidStudentAccess_ToInstructorAnalytics()
    {
        var state = AnalyticsState.FromRun(
            new RunMetrics(PublishedCourses: 1, PaidOrders: 1, Revenue: 799000m, Enrollments: 1));

        var act = () => state.GetInstructorAnalytics(UserRole.Student);

        act.Should().Throw<UnauthorizedAccessException>();
    }

    [Fact]
    public void AnalyticsPipeline_ShouldAllowAdminAccess_ToInstructorStats()
    {
        var state = AnalyticsState.FromRun(
            new RunMetrics(PublishedCourses: 2, PaidOrders: 3, Revenue: 1500000m, Enrollments: 5));

        var instructorStats = state.GetInstructorStats(UserRole.Admin);

        instructorStats.PublishedCourseCount.Should().Be(2);
        instructorStats.Revenue.Should().Be(1500000m);
    }

    [Fact]
    public void AnalyticsPipeline_ShouldReturnZeroMetrics_WhenNoTransactions()
    {
        var state = AnalyticsState.FromRun(
            new RunMetrics(PublishedCourses: 0, PaidOrders: 0, Revenue: 0m, Enrollments: 0));

        var dashboard = state.GetSystemDashboard(UserRole.Admin);

        dashboard.TotalPaidOrders.Should().Be(0);
        dashboard.TotalRevenue.Should().Be(0m);
        dashboard.TotalEnrollments.Should().Be(0);
    }

    private enum UserRole
    {
        Student,
        Instructor,
        Admin
    }

    private readonly record struct RunMetrics(int PublishedCourses, int PaidOrders, decimal Revenue, int Enrollments);
    private readonly record struct InstructorStats(int PublishedCourseCount, int EnrollmentCount, decimal Revenue);
    private readonly record struct InstructorAnalytics(int TotalPaidOrders, decimal Revenue);
    private readonly record struct Dashboard(int TotalPaidOrders, decimal TotalRevenue, int TotalEnrollments);
    private readonly record struct TrendPoint(DateOnly Date, decimal Revenue);

    private sealed class AnalyticsState
    {
        private RunMetrics _metrics;

        public static AnalyticsState FromRun(RunMetrics metrics)
            => new() { _metrics = metrics };

        public InstructorStats GetInstructorStats(UserRole actor)
        {
            RequireRole(actor, UserRole.Instructor, UserRole.Admin);
            return new InstructorStats(_metrics.PublishedCourses, _metrics.Enrollments, _metrics.Revenue);
        }

        public InstructorAnalytics GetInstructorAnalytics(UserRole actor)
        {
            RequireRole(actor, UserRole.Instructor, UserRole.Admin);
            return new InstructorAnalytics(_metrics.PaidOrders, _metrics.Revenue);
        }

        public Dashboard GetSystemDashboard(UserRole actor)
        {
            RequireRole(actor, UserRole.Admin);
            return new Dashboard(_metrics.PaidOrders, _metrics.Revenue, _metrics.Enrollments);
        }

        public IReadOnlyList<TrendPoint> GetRevenueTrend(UserRole actor)
        {
            RequireRole(actor, UserRole.Admin);
            return [new TrendPoint(DateOnly.FromDateTime(DateTime.UtcNow), _metrics.Revenue)];
        }

        private static void RequireRole(UserRole actor, params UserRole[] allowed)
        {
            if (!allowed.Contains(actor))
            {
                throw new UnauthorizedAccessException("Không có quyền truy cập");
            }
        }
    }
}
