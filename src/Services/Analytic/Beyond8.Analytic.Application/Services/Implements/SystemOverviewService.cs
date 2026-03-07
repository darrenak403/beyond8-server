using Beyond8.Analytic.Application.Clients.Catalog;
using Beyond8.Analytic.Application.Clients.Identity;
using Beyond8.Analytic.Application.Clients.Learning;
using Beyond8.Analytic.Application.Clients.Sale;
using Beyond8.Analytic.Application.Dtos.SystemOverview;
using Beyond8.Analytic.Application.Services.Interfaces;
using Beyond8.Analytic.Domain.Enums;
using Beyond8.Analytic.Domain.Repositories.Interfaces;
using Beyond8.Common.Utilities;
using Microsoft.Extensions.Logging;

namespace Beyond8.Analytic.Application.Services.Implements;

public class SystemOverviewService(
    ILogger<SystemOverviewService> logger,
    IUnitOfWork unitOfWork,
    ISaleClient saleClient,
    IIdentityClient identityClient,
    ICatalogClient catalogClient,
    ILearningClient learningClient) : ISystemOverviewService
{
    public async Task<ApiResponse<SystemDashboardResponse>> GetSystemDashboardAsync()
    {
        var now = DateTime.UtcNow;

        var overview = await unitOfWork.AggSystemOverviewRepository.GetCurrentAsync();
        var monthly12 = await unitOfWork.AggSystemOverviewMonthlyRepository.GetLastNMonthsAsync(12);

        var currentYearRecords = monthly12.Where(m => m.Year == now.Year).ToList();

        // Gọi song song tới 3 service để lấy số liệu thực từ database
        var userTask = identityClient.GetPlatformUserStatsAsync();
        var courseTask = catalogClient.GetPlatformCourseStatsAsync();
        var enrollmentTask = learningClient.GetPlatformEnrollmentStatsAsync();
        await Task.WhenAll(userTask, courseTask, enrollmentTask);

        var userStats = userTask.Result.IsSuccess ? userTask.Result.Data : null;
        var courseStats = courseTask.Result.IsSuccess ? courseTask.Result.Data : null;
        var enrollmentStats = enrollmentTask.Result.IsSuccess ? enrollmentTask.Result.Data : null;

        if (userStats == null)
            logger.LogWarning("Could not fetch user stats from Identity; falling back to aggregate");
        if (courseStats == null)
            logger.LogWarning("Could not fetch course stats from Catalog; falling back to aggregate");
        if (enrollmentStats == null)
            logger.LogWarning("Could not fetch enrollment stats from Learning; falling back to aggregate");

        var dashboard = new SystemDashboardResponse
        {
            TotalUsers = userStats?.TotalUsers
                         ?? (overview?.TotalInstructors ?? 0) + (overview?.TotalStudents ?? 0),
            TotalInstructors = userStats?.TotalInstructors ?? overview?.TotalInstructors ?? 0,
            TotalStudents = userStats?.TotalStudents ?? overview?.TotalStudents ?? 0,
            TotalCourses = courseStats?.TotalCourses ?? overview?.TotalCourses ?? 0,
            TotalPublishedCourses = courseStats?.TotalPublishedCourses ?? overview?.TotalPublishedCourses ?? 0,
            TotalEnrollments = enrollmentStats?.TotalEnrollments ?? overview?.TotalEnrollments ?? 0,
            TotalCompletedEnrollments = enrollmentStats?.TotalCompletedEnrollments ?? overview?.TotalCompletedEnrollments ?? 0,
            TotalPlatformFee = overview?.TotalPlatformFee ?? 0,
            AvgCourseRating = overview?.AvgCourseRating ?? 0,
            UpdatedAt = overview?.UpdatedAt
        };

        logger.LogInformation("System dashboard fetched for {YearMonth}", $"{now.Year:D4}-{now.Month:D2}");
        return ApiResponse<SystemDashboardResponse>.SuccessResponse(dashboard, "Lấy dashboard hệ thống thành công");
    }

    public async Task<ApiResponse<RevenueTrendResponse>> GetRevenueTrendAsync(RevenueTrendRequest request)
    {
        var year = request.Year;

        switch (request.GroupBy)
        {
            case GroupByPeriod.Year:
                {
                    var records = await unitOfWork.AggSystemOverviewMonthlyRepository.GetByYearAsync(year!.Value);
                    var lookup = records.ToDictionary(r => r.Month);

                    var dataPoints = Enumerable.Range(1, 12).Select(m =>
                    {
                        lookup.TryGetValue(m, out var rec);
                        return new RevenueDataPoint
                        {
                            Period = $"{year:D4}-{m:D2}",
                            Label = $"Tháng {m}",
                            Revenue = rec?.Revenue ?? 0,
                            Profit = rec?.PlatformProfit ?? 0,
                            InstructorEarnings = rec?.InstructorEarnings ?? 0,
                            NewEnrollments = rec?.NewEnrollments ?? 0
                        };
                    }).ToList();

                    var response = new RevenueTrendResponse
                    {
                        PeriodLabel = $"Năm {year}",
                        TotalRevenue = dataPoints.Sum(d => d.Revenue),
                        TotalProfit = dataPoints.Sum(d => d.Profit),
                        TotalInstructorEarnings = dataPoints.Sum(d => d.InstructorEarnings),
                        TotalNewEnrollments = dataPoints.Sum(d => d.NewEnrollments),
                        DataPoints = dataPoints
                    };

                    logger.LogInformation("Revenue trend (Year) fetched for {Year}", year);
                    return ApiResponse<RevenueTrendResponse>.SuccessResponse(response, "Lấy xu hướng doanh thu thành công");
                }

            case GroupByPeriod.Quarter:
                {
                    var quarter = request.Quarter!.Value;
                    var records = await unitOfWork.AggSystemOverviewMonthlyRepository.GetByQuarterAsync(year!.Value, quarter);
                    var lookup = records.ToDictionary(r => r.Month);

                    var startMonth = (quarter - 1) * 3 + 1;
                    var dataPoints = Enumerable.Range(startMonth, 3).Select(m =>
                    {
                        lookup.TryGetValue(m, out var rec);
                        return new RevenueDataPoint
                        {
                            Period = $"{year:D4}-{m:D2}",
                            Label = $"Tháng {m}",
                            Revenue = rec?.Revenue ?? 0,
                            Profit = rec?.PlatformProfit ?? 0,
                            InstructorEarnings = rec?.InstructorEarnings ?? 0,
                            NewEnrollments = rec?.NewEnrollments ?? 0
                        };
                    }).ToList();

                    var response = new RevenueTrendResponse
                    {
                        PeriodLabel = $"Q{quarter}/{year}",
                        TotalRevenue = dataPoints.Sum(d => d.Revenue),
                        TotalProfit = dataPoints.Sum(d => d.Profit),
                        TotalInstructorEarnings = dataPoints.Sum(d => d.InstructorEarnings),
                        TotalNewEnrollments = dataPoints.Sum(d => d.NewEnrollments),
                        DataPoints = dataPoints
                    };

                    logger.LogInformation("Revenue trend (Quarter) fetched for Q{Quarter}/{Year}", quarter, year);
                    return ApiResponse<RevenueTrendResponse>.SuccessResponse(response, "Lấy xu hướng doanh thu thành công");
                }

            case GroupByPeriod.Month:
                {
                    var month = request.Month!.Value;
                    var records = await unitOfWork.AggSystemOverviewDailyRepository.GetByMonthAsync(year!.Value, month);
                    var lookup = records.ToDictionary(r => r.Day);
                    var daysInMonth = DateTime.DaysInMonth(year.Value, month);

                    var dataPoints = Enumerable.Range(1, daysInMonth).Select(d =>
                    {
                        lookup.TryGetValue(d, out var rec);
                        return new RevenueDataPoint
                        {
                            Period = $"{year:D4}-{month:D2}-{d:D2}",
                            Label = $"{d}/{month}",
                            Revenue = rec?.Revenue ?? 0,
                            Profit = rec?.PlatformProfit ?? 0,
                            InstructorEarnings = rec?.InstructorEarnings ?? 0,
                            NewEnrollments = rec?.NewEnrollments ?? 0
                        };
                    }).ToList();

                    var response = new RevenueTrendResponse
                    {
                        PeriodLabel = $"Tháng {month}/{year}",
                        TotalRevenue = dataPoints.Sum(dp => dp.Revenue),
                        TotalProfit = dataPoints.Sum(dp => dp.Profit),
                        TotalInstructorEarnings = dataPoints.Sum(dp => dp.InstructorEarnings),
                        TotalNewEnrollments = dataPoints.Sum(dp => dp.NewEnrollments),
                        DataPoints = dataPoints
                    };

                    logger.LogInformation("Revenue trend (Month) fetched for {Month}/{Year}", month, year);
                    return ApiResponse<RevenueTrendResponse>.SuccessResponse(response, "Lấy xu hướng doanh thu thành công");
                }

            case GroupByPeriod.Custom:
                {
                    var start = request.StartDate!.Value;
                    var end = request.EndDate!.Value;
                    var fromKey = $"{start:yyyy-MM-dd}";
                    var toKey = $"{end:yyyy-MM-dd}";

                    var records = await unitOfWork.AggSystemOverviewDailyRepository.GetByDateRangeAsync(fromKey, toKey);
                    var lookup = records.ToDictionary(r => r.DateKey);

                    // Generate all dates in range
                    var dataPoints = new List<RevenueDataPoint>();
                    for (var date = start.Date; date <= end.Date; date = date.AddDays(1))
                    {
                        var key = $"{date:yyyy-MM-dd}";
                        lookup.TryGetValue(key, out var rec);
                        dataPoints.Add(new RevenueDataPoint
                        {
                            Period = key,
                            Label = $"{date.Day}/{date.Month}",
                            Revenue = rec?.Revenue ?? 0,
                            Profit = rec?.PlatformProfit ?? 0,
                            InstructorEarnings = rec?.InstructorEarnings ?? 0,
                            NewEnrollments = rec?.NewEnrollments ?? 0
                        });
                    }

                    var response = new RevenueTrendResponse
                    {
                        PeriodLabel = $"{start:dd/MM/yyyy} – {end:dd/MM/yyyy}",
                        TotalRevenue = dataPoints.Sum(dp => dp.Revenue),
                        TotalProfit = dataPoints.Sum(dp => dp.Profit),
                        TotalInstructorEarnings = dataPoints.Sum(dp => dp.InstructorEarnings),
                        TotalNewEnrollments = dataPoints.Sum(dp => dp.NewEnrollments),
                        DataPoints = dataPoints
                    };

                    logger.LogInformation("Revenue trend (Custom) fetched from {From} to {To}", fromKey, toKey);
                    return ApiResponse<RevenueTrendResponse>.SuccessResponse(response, "Lấy xu hướng doanh thu thành công");
                }

            default:
                return ApiResponse<RevenueTrendResponse>.FailureResponse("Loại nhóm không hợp lệ");
        }
    }

    public async Task<ApiResponse<BackfillRevenueResponse>> BackfillRevenueAsync(DateTime from, DateTime to)
    {
        var result = await saleClient.GetRevenueByDateRangeAsync(from, to);
        if (!result.IsSuccess || result.Data == null)
        {
            logger.LogWarning("Backfill: Sale service returned failure for {From} – {To}", from, to);
            return ApiResponse<BackfillRevenueResponse>.FailureResponse($"Không lấy được dữ liệu từ Sale Service: {result.Message}");
        }

        var summaries = result.Data;
        int dailyCount = 0;

        // Track which year-month combos we've touched for monthly upsert
        var monthlyTotals = new Dictionary<string, (int Year, int Month, decimal Revenue, decimal PlatformFee, decimal InstructorEarnings, int Enrollments)>();

        foreach (var s in summaries)
        {
            // Upsert daily record
            var daily = await unitOfWork.AggSystemOverviewDailyRepository.GetOrCreateForDateAsync(s.DateKey, s.Year, s.Month, s.Day);
            daily.Revenue += s.Revenue;
            daily.PlatformProfit += s.PlatformFee;
            daily.InstructorEarnings += s.InstructorEarnings;
            daily.NewEnrollments += s.NewEnrollments;
            dailyCount++;

            // Accumulate monthly
            var ymKey = $"{s.Year:D4}-{s.Month:D2}";
            if (!monthlyTotals.TryGetValue(ymKey, out var mt))
                mt = (s.Year, s.Month, 0, 0, 0, 0);

            monthlyTotals[ymKey] = (
                mt.Year, mt.Month,
                mt.Revenue + s.Revenue,
                mt.PlatformFee + s.PlatformFee,
                mt.InstructorEarnings + s.InstructorEarnings,
                mt.Enrollments + s.NewEnrollments
            );
        }

        // Upsert monthly records
        foreach (var (ymKey, mt) in monthlyTotals)
        {
            var monthly = await unitOfWork.AggSystemOverviewMonthlyRepository.GetOrCreateForMonthAsync(ymKey, mt.Year, mt.Month);
            monthly.Revenue += mt.Revenue;
            monthly.PlatformProfit += mt.PlatformFee;
            monthly.InstructorEarnings += mt.InstructorEarnings;
            monthly.NewEnrollments += mt.Enrollments;
        }

        await unitOfWork.SaveChangesAsync();

        var response = new BackfillRevenueResponse
        {
            DailyRecordsUpserted = dailyCount,
            MonthlyRecordsUpserted = monthlyTotals.Count,
            TotalRevenue = summaries.Sum(s => s.Revenue),
            TotalPlatformFee = summaries.Sum(s => s.PlatformFee),
            TotalInstructorEarnings = summaries.Sum(s => s.InstructorEarnings),
            TotalEnrollments = summaries.Sum(s => s.NewEnrollments)
        };

        logger.LogInformation("Backfill completed: {DailyCount} daily + {MonthlyCount} monthly records upserted", dailyCount, monthlyTotals.Count);
        return ApiResponse<BackfillRevenueResponse>.SuccessResponse(response, "Backfill doanh thu thành công");
    }
}
