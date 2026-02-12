using Beyond8.Analytic.Application.Dtos.Common;
using Beyond8.Analytic.Application.Dtos.InstructorRevenue;
using Beyond8.Common.Utilities;

namespace Beyond8.Analytic.Application.Services.Interfaces;

public interface IInstructorRevenueService
{
    Task<ApiResponse<List<InstructorRevenueResponse>>> GetAllInstructorRevenueAsync(DateRangeAnalyticRequest request);
    Task<ApiResponse<InstructorRevenueResponse>> GetInstructorRevenueAsync(Guid instructorId);
    Task<ApiResponse<List<TopInstructorResponse>>> GetTopInstructorsAsync(int count = 10, string sortBy = "revenue");
}
