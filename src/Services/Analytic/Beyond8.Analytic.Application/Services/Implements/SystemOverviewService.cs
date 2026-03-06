using Beyond8.Analytic.Application.Dtos.SystemOverview;
using Beyond8.Analytic.Application.Services.Interfaces;
using Beyond8.Analytic.Domain.Repositories.Interfaces;
using Beyond8.Common.Utilities;
using Microsoft.Extensions.Logging;

namespace Beyond8.Analytic.Application.Services.Implements;

public class SystemOverviewService(
    ILogger<SystemOverviewService> logger,
    IUnitOfWork unitOfWork) : ISystemOverviewService
{
    public async Task<ApiResponse<SystemDashboardResponse>> GetSystemDashboardAsync()
    {
        var now = DateTime.UtcNow;

        var overview = await unitOfWork.AggSystemOverviewRepository.GetCurrentAsync();
        var monthly12 = await unitOfWork.AggSystemOverviewMonthlyRepository.GetLastNMonthsAsync(12);

        var currentYearRecords = monthly12.Where(m => m.Year == now.Year).ToList();

        var dashboard = new SystemDashboardResponse
        {
            TotalUsers = overview?.TotalUsers ?? 0,
            TotalInstructors = overview?.TotalInstructors ?? 0,
            TotalStudents = overview?.TotalStudents ?? 0,
            TotalCourses = overview?.TotalCourses ?? 0,
            TotalPublishedCourses = overview?.TotalPublishedCourses ?? 0,
            TotalEnrollments = overview?.TotalEnrollments ?? 0,
            TotalCompletedEnrollments = overview?.TotalCompletedEnrollments ?? 0,
            TotalRevenue = overview?.TotalRevenue ?? 0,
            TotalPlatformFee = overview?.TotalPlatformFee ?? 0,
            TotalInstructorEarnings = overview?.TotalInstructorEarnings ?? 0,
            AvgCourseRating = overview?.AvgCourseRating ?? 0,
            CurrentYearRevenue = currentYearRecords.Sum(m => m.Revenue),
            CurrentYearProfit = currentYearRecords.Sum(m => m.PlatformProfit),
            RevenueTrend12M = monthly12.Select(m => new MonthlyDataPoint
            {
                YearMonth = m.YearMonth,
                Label = $"Tháng {m.Month}",
                Revenue = m.Revenue,
                Profit = m.PlatformProfit
            }).ToList(),
            CashflowTrend6M = monthly12.TakeLast(6).Select(m => new MonthlyDataPoint
            {
                YearMonth = m.YearMonth,
                Label = $"T{m.Month}",
                Revenue = m.Revenue,
                Profit = m.PlatformProfit
            }).ToList(),
            UpdatedAt = overview?.UpdatedAt
        };

        logger.LogInformation("System dashboard fetched for {YearMonth}", $"{now.Year:D4}-{now.Month:D2}");
        return ApiResponse<SystemDashboardResponse>.SuccessResponse(dashboard, "Lấy dashboard hệ thống thành công");
    }
}
