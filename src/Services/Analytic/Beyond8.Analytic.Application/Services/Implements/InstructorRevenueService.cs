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
    IUnitOfWork unitOfWork) : IInstructorRevenueService
{
    public async Task<ApiResponse<List<InstructorRevenueResponse>>> GetAllInstructorRevenueAsync(DateRangeAnalyticRequest request)
    {
        var result = await unitOfWork.AggInstructorRevenueRepository.GetPagedAsync(
            pageNumber: request.PageNumber,
            pageSize: request.PageSize,
            filter: e => e.IsCurrent,
            orderBy: q => q.OrderByDescending(e => e.TotalRevenue));

        var items = result.Items.Select(e => e.ToResponse()).ToList();

        return ApiResponse<List<InstructorRevenueResponse>>.SuccessPagedResponse(
            items, result.TotalCount, request.PageNumber, request.PageSize,
            "Lấy thống kê doanh thu giảng viên thành công");
    }

    public async Task<ApiResponse<InstructorRevenueResponse>> GetInstructorRevenueAsync(Guid instructorId)
    {
        var revenue = await unitOfWork.AggInstructorRevenueRepository.GetByInstructorIdAsync(instructorId);
        if (revenue == null)
            return ApiResponse<InstructorRevenueResponse>.FailureResponse("Không tìm thấy thống kê doanh thu");

        return ApiResponse<InstructorRevenueResponse>.SuccessResponse(revenue.ToResponse(), "Lấy thống kê doanh thu thành công");
    }

    public async Task<ApiResponse<List<TopInstructorResponse>>> GetTopInstructorsAsync(int count = 10, string sortBy = "revenue")
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
}
