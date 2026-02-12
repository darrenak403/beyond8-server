using Beyond8.Analytic.Application.Dtos.Common;
using Beyond8.Analytic.Application.Dtos.CourseStats;
using Beyond8.Analytic.Application.Mappings;
using Beyond8.Analytic.Application.Services.Interfaces;
using Beyond8.Analytic.Domain.Repositories.Interfaces;
using Beyond8.Common.Utilities;
using Microsoft.Extensions.Logging;

namespace Beyond8.Analytic.Application.Services.Implements;

public class CourseStatsService(
    ILogger<CourseStatsService> logger,
    IUnitOfWork unitOfWork) : ICourseStatsService
{
    public async Task<ApiResponse<List<CourseStatsResponse>>> GetAllCourseStatsAsync(DateRangeAnalyticRequest request)
    {
        var result = await unitOfWork.AggCourseStatsRepository.GetPagedAsync(
            pageNumber: request.PageNumber,
            pageSize: request.PageSize,
            filter: e => e.IsCurrent,
            orderBy: q => q.OrderByDescending(e => e.TotalStudents));

        var items = result.Items.Select(e => e.ToResponse()).ToList();

        return ApiResponse<List<CourseStatsResponse>>.SuccessPagedResponse(
            items, result.TotalCount, request.PageNumber, request.PageSize,
            "Lấy thống kê khóa học thành công");
    }

    public async Task<ApiResponse<CourseStatsResponse>> GetCourseStatsByIdAsync(Guid courseId)
    {
        var stats = await unitOfWork.AggCourseStatsRepository.GetByCourseIdAsync(courseId);
        if (stats == null)
            return ApiResponse<CourseStatsResponse>.FailureResponse("Không tìm thấy thống kê khóa học");

        return ApiResponse<CourseStatsResponse>.SuccessResponse(stats.ToResponse(), "Lấy thống kê khóa học thành công");
    }

    public async Task<ApiResponse<List<TopCourseResponse>>> GetTopCoursesAsync(int count = 10, string sortBy = "students")
    {
        var result = await unitOfWork.AggCourseStatsRepository.GetPagedAsync(
            pageNumber: 1,
            pageSize: count,
            filter: e => e.IsCurrent,
            orderBy: sortBy.ToLower() switch
            {
                "revenue" => q => q.OrderByDescending(e => e.TotalRevenue),
                "rating" => q => q.OrderByDescending(e => e.AvgRating),
                _ => q => q.OrderByDescending(e => e.TotalStudents)
            });

        var items = result.Items.Select(e => e.ToTopResponse()).ToList();
        return ApiResponse<List<TopCourseResponse>>.SuccessResponse(items, "Lấy top khóa học thành công");
    }

    public async Task<ApiResponse<List<CourseStatsResponse>>> GetInstructorCourseStatsAsync(
        Guid instructorId, PaginationRequest pagination)
    {
        var result = await unitOfWork.AggCourseStatsRepository.GetPagedAsync(
            pageNumber: pagination.PageNumber,
            pageSize: pagination.PageSize,
            filter: e => e.InstructorId == instructorId && e.IsCurrent,
            orderBy: q => q.OrderByDescending(e => e.TotalStudents));

        var items = result.Items.Select(e => e.ToResponse()).ToList();

        return ApiResponse<List<CourseStatsResponse>>.SuccessPagedResponse(
            items, result.TotalCount, pagination.PageNumber, pagination.PageSize,
            "Lấy thống kê khóa học của giảng viên thành công");
    }
}
