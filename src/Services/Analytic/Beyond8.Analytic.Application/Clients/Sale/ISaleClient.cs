using Beyond8.Analytic.Application.Dtos.Sale;
using Beyond8.Analytic.Application.Dtos.Stats;
using Beyond8.Common.Clients;
using Beyond8.Common.Utilities;

namespace Beyond8.Analytic.Application.Clients.Sale;

public interface ISaleClient : IBaseClient
{
    Task<ApiResponse<List<DailyRevenueSummary>>> GetRevenueByDateRangeAsync(DateTime from, DateTime to);
    Task<ApiResponse<List<DailyRevenueSummary>>> GetInstructorRevenueByDateRangeAsync(Guid instructorId, DateTime from, DateTime to);
    Task<ApiResponse<InstructorWalletStatsResponse>> GetInstructorWalletAsync(Guid instructorId);
}
