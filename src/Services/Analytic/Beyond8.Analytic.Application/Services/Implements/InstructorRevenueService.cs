using Beyond8.Analytic.Application.Clients.Catalog;
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
    ISaleClient saleClient) : IInstructorRevenueService
{
    public async Task<ApiResponse<MyRevenueResponse>> GetInstructorRevenueAsync(Guid instructorId)
    {
        try
        {
            // Fetch aggregate (for InstructorName + status fields not in external APIs) and real-time data in parallel
            var aggregateTask = unitOfWork.AggInstructorRevenueRepository.GetByInstructorIdAsync(instructorId);
            var courseStatsTask = catalogClient.GetInstructorCourseStatsAsync(instructorId);
            var walletTask = saleClient.GetInstructorWalletAsync(instructorId);

            await Task.WhenAll(aggregateTask, courseStatsTask, walletTask);

            var revenue = aggregateTask.Result;
            var courseStatsResult = courseStatsTask.Result;
            var walletResult = walletTask.Result;

            var courseStats = courseStatsResult.IsSuccess ? courseStatsResult.Data : null;
            var wallet = walletResult.IsSuccess ? walletResult.Data : null;

            if (courseStats == null)
                logger.LogWarning("Catalog service unavailable for instructor {InstructorId}, using aggregate fallback", instructorId);
            if (wallet == null)
                logger.LogWarning("Sale service unavailable for instructor {InstructorId}, using aggregate fallback", instructorId);

            if (revenue == null && courseStats == null && wallet == null)
                return ApiResponse<MyRevenueResponse>.FailureResponse("Không tìm thấy thống kê doanh thu");

            var response = new MyRevenueResponse
            {
                InstructorId = instructorId,
                InstructorName = revenue?.InstructorName ?? string.Empty,
                // Course counts — prefer real-time Catalog data
                TotalCourses = courseStats?.TotalCourses ?? revenue?.TotalCourses ?? 0,
                DraftCourses = courseStats?.DraftCourses ?? revenue?.DraftCourses ?? 0,
                PendingApprovalCourses = courseStats?.PendingApprovalCourses ?? revenue?.PendingApprovalCourses ?? 0,
                PublishedCourses = courseStats?.PublishedCourses ?? revenue?.PublishedCourses ?? 0,
                RejectedCourses = courseStats?.RejectedCourses ?? revenue?.RejectedCourses ?? 0,
                // Status fields not tracked by Catalog stats endpoint — use aggregate only
                ApprovedCourses = revenue?.ApprovedCourses ?? 0,
                ArchivedCourses = revenue?.ArchivedCourses ?? 0,
                SuspendedCourses = revenue?.SuspendedCourses ?? 0,
                // Student & rating — prefer real-time Catalog data
                TotalStudents = courseStats?.TotalStudents ?? revenue?.TotalStudents ?? 0,
                AvgCourseRating = courseStats?.AverageRating ?? revenue?.AvgCourseRating ?? 0,
                TotalReviews = courseStats?.TotalReviews ?? revenue?.TotalReviews ?? 0,
                // Financial — prefer real-time Sale wallet data
                TotalInstructorEarnings = wallet?.TotalEarnings ?? revenue?.TotalInstructorEarnings ?? 0,
                AvailableBalance = wallet?.AvailableBalance ?? revenue?.AvailableBalance ?? 0,
                SnapshotDate = revenue?.SnapshotDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
                UpdatedAt = revenue?.UpdatedAt
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
}
