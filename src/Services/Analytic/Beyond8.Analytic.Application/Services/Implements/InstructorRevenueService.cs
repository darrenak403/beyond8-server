using Beyond8.Analytic.Application.Clients.Catalog;
using Beyond8.Analytic.Application.Clients.Learning;
using Beyond8.Analytic.Application.Clients.Sale;
using Beyond8.Analytic.Application.Dtos.Common;
using Beyond8.Analytic.Application.Dtos.InstructorRevenue;
using Beyond8.Analytic.Application.Mappings;
using Beyond8.Analytic.Application.Services.Interfaces;
using Beyond8.Analytic.Domain.Repositories.Interfaces;
using Beyond8.Common.Utilities;
using Microsoft.Extensions.Logging;

namespace Beyond8.Analytic.Application.Services.Implements;

public class InstructorRevenueService(
    ILogger<InstructorRevenueService> logger,
    IUnitOfWork unitOfWork,
    ICatalogClient catalogClient,
    ISaleClient saleClient,
    ILearningClient learningClient) : IInstructorRevenueService
{
    public async Task<ApiResponse<MyRevenueResponse>> GetInstructorRevenueAsync(Guid instructorId)
    {
        try
        {
            var now = DateTime.UtcNow;
            var thisMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var lastMonthStart = thisMonthStart.AddMonths(-1);
            var lastMonthEnd = thisMonthStart.AddSeconds(-1);

            // All calls in parallel
            var aggregateTask = unitOfWork.AggInstructorRevenueRepository.GetByInstructorIdAsync(instructorId);
            var courseStatsTask = catalogClient.GetInstructorCourseStatsAsync(instructorId);
            var walletTask = saleClient.GetInstructorWalletAsync(instructorId);
            var enrollmentTask = learningClient.GetInstructorEnrollmentStatsAsync(instructorId);
            var revThisMonthTask = saleClient.GetInstructorRevenueByDateRangeAsync(instructorId, thisMonthStart, now);
            var revLastMonthTask = saleClient.GetInstructorRevenueByDateRangeAsync(instructorId, lastMonthStart, lastMonthEnd);

            await Task.WhenAll(aggregateTask, courseStatsTask, walletTask, enrollmentTask, revThisMonthTask, revLastMonthTask);

            var revenue = aggregateTask.Result;
            var courseStatsResult = courseStatsTask.Result;
            var walletResult = walletTask.Result;
            var enrollmentResult = enrollmentTask.Result;
            var revThisMonthResult = revThisMonthTask.Result;
            var revLastMonthResult = revLastMonthTask.Result;

            var courseStats = courseStatsResult.IsSuccess ? courseStatsResult.Data : null;
            var wallet = walletResult.IsSuccess ? walletResult.Data : null;
            var enrollment = enrollmentResult.IsSuccess ? enrollmentResult.Data : null;

            if (courseStats == null)
                logger.LogWarning("Catalog service unavailable for instructor {InstructorId}, using aggregate fallback", instructorId);
            if (wallet == null)
                logger.LogWarning("Sale service unavailable for instructor {InstructorId}, using aggregate fallback", instructorId);
            if (enrollment == null)
                logger.LogWarning("Learning service unavailable for instructor {InstructorId}, enrollment stats missing", instructorId);

            if (revenue == null && courseStats == null && wallet == null)
                return ApiResponse<MyRevenueResponse>.FailureResponse("Không tìm thấy thống kê doanh thu");

            // Monthly revenue
            var revenueThisMonth = revThisMonthResult.IsSuccess
                ? revThisMonthResult.Data?.Sum(d => d.InstructorEarnings) ?? 0m
                : 0m;
            var revenueLastMonth = revLastMonthResult.IsSuccess
                ? revLastMonthResult.Data?.Sum(d => d.InstructorEarnings) ?? 0m
                : 0m;

            var coursesThisMonth = courseStats?.CoursesThisMonth ?? 0;
            var coursesLastMonth = courseStats?.CoursesLastMonth ?? 0;
            var studentsThisMonth = enrollment?.StudentsThisMonth ?? 0;
            var studentsLastMonth = enrollment?.StudentsLastMonth ?? 0;

            var response = new MyRevenueResponse
            {
                InstructorId = instructorId,
                InstructorName = revenue?.InstructorName ?? string.Empty,
                SnapshotDate = revenue?.SnapshotDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
                UpdatedAt = revenue?.UpdatedAt,
                Courses = new CourseStatsGroup
                {
                    // Prefer real-time Catalog data; fall back to snapshot aggregate
                    Total = courseStats?.TotalCourses ?? revenue?.TotalCourses ?? 0,
                    Draft = courseStats?.DraftCourses ?? revenue?.DraftCourses ?? 0,
                    PendingApproval = courseStats?.PendingApprovalCourses ?? revenue?.PendingApprovalCourses ?? 0,
                    Approved = courseStats?.ApprovedCourses ?? revenue?.ApprovedCourses ?? 0,
                    Published = courseStats?.PublishedCourses ?? revenue?.PublishedCourses ?? 0,
                    Rejected = courseStats?.RejectedCourses ?? revenue?.RejectedCourses ?? 0,
                    Archived = revenue?.ArchivedCourses ?? 0,
                    Suspended = revenue?.SuspendedCourses ?? 0,
                    PublishedThisMonth = coursesThisMonth,
                    PublishedLastMonth = coursesLastMonth,
                    PublishedGrowthPercent = GrowthPercent(coursesThisMonth, coursesLastMonth),
                    PublishedGrowthAbsolute = coursesThisMonth - coursesLastMonth,
                },
                Students = new StudentStatsGroup
                {
                    // DISTINCT users from Learning; fall back to Catalog/aggregate
                    Total = enrollment?.TotalStudents ?? courseStats?.TotalStudents ?? revenue?.TotalStudents ?? 0,
                    ThisMonth = studentsThisMonth,
                    LastMonth = studentsLastMonth,
                    GrowthPercent = GrowthPercent(studentsThisMonth, studentsLastMonth),
                    GrowthAbsolute = studentsThisMonth - studentsLastMonth,
                },
                Revenue = new RevenueStatsGroup
                {
                    // Prefer real-time Sale wallet data
                    TotalEarnings = wallet?.TotalEarnings ?? revenue?.TotalInstructorEarnings ?? 0,
                    AvailableBalance = wallet?.AvailableBalance ?? revenue?.AvailableBalance ?? 0,
                    ThisMonth = revenueThisMonth,
                    LastMonth = revenueLastMonth,
                    GrowthPercent = GrowthPercent(revenueThisMonth, revenueLastMonth),
                    GrowthAbsolute = revenueThisMonth - revenueLastMonth,
                },
                Rating = new RatingGroup
                {
                    Average = courseStats?.AverageRating ?? revenue?.AvgCourseRating ?? 0,
                    TotalReviews = courseStats?.TotalReviews ?? revenue?.TotalReviews ?? 0,
                },
            };

            return ApiResponse<MyRevenueResponse>.SuccessResponse(response, "Lấy thống kê doanh thu thành công");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get instructor revenue. InstructorId={InstructorId}", instructorId);
            throw;
        }
    }

    public async Task<ApiResponse<List<TopInstructorResponse>>> GetTopInstructorsAsync(int count = 10, string sortBy = "revenue")
    {
        try
        {
            var result = await unitOfWork.AggInstructorRevenueRepository.GetPagedAsync(
                pageNumber: 1,
                pageSize: count,
                filter: e => e.IsCurrent,
                orderBy: sortBy.ToLower() switch
                {
                    "students" => q => q.OrderByDescending(e => e.TotalStudents),
                    "rating" => q => q.OrderByDescending(e => e.AvgCourseRating),
                    _ => q => q.OrderByDescending(e => e.TotalRevenue)
                });

            var items = result.Items.Select(e => e.ToTopResponse()).ToList();
            return ApiResponse<List<TopInstructorResponse>>.SuccessResponse(items, "Lấy top giảng viên thành công");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get top instructors. Count={Count}, SortBy={SortBy}", count, sortBy);
            throw;
        }
    }

    private static decimal GrowthPercent(decimal current, decimal prev)
        => prev == 0 ? (current > 0 ? 100m : 0m) : Math.Round((current - prev) * 100m / prev, 1);

    private static decimal GrowthPercent(int current, int prev)
        => prev == 0 ? (current > 0 ? 100m : 0m) : Math.Round((current - prev) * 100m / prev, 1);
}
