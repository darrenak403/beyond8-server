using Beyond8.Analytic.Application.Dtos.Sale;
using Beyond8.Analytic.Application.Dtos.Stats;
using Beyond8.Common.Clients;
using Beyond8.Common.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Beyond8.Analytic.Application.Clients.Sale;

public class SaleClient(
    HttpClient httpClient,
    IHttpContextAccessor httpContextAccessor,
    ILogger<SaleClient> logger)
    : BaseClient(httpClient, httpContextAccessor), ISaleClient
{
    public async Task<ApiResponse<List<DailyRevenueSummary>>> GetRevenueByDateRangeAsync(DateTime from, DateTime to)
    {
        try
        {
            var fromStr = from.Date.ToString("yyyy-MM-dd");
            var toStr = to.Date.ToString("yyyy-MM-dd");
            var data = await GetAsync<List<DailyRevenueSummary>>(
                $"/api/v1/internal/orders/revenue-by-date?from={fromStr}&to={toStr}");
            return ApiResponse<List<DailyRevenueSummary>>.SuccessResponse(data, "OK");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to call Sale Service GetRevenueByDateRange: {From} – {To}", from, to);
            return ApiResponse<List<DailyRevenueSummary>>.FailureResponse(ex.Message);
        }
    }
    public async Task<ApiResponse<InstructorWalletStatsResponse>> GetInstructorWalletAsync(Guid instructorId)
    {
        try
        {
            var data = await GetAsync<InstructorWalletStatsResponse>(
                $"/api/v1/internal/wallets/instructors/{instructorId}");
            return ApiResponse<InstructorWalletStatsResponse>.SuccessResponse(data, "OK");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get instructor wallet from Sale Service. InstructorId={InstructorId}", instructorId);
            return ApiResponse<InstructorWalletStatsResponse>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<DailyRevenueSummary>>> GetInstructorRevenueByDateRangeAsync(Guid instructorId, DateTime from, DateTime to)
    {
        try
        {
            var fromStr = from.Date.ToString("yyyy-MM-dd");
            var toStr = to.Date.ToString("yyyy-MM-dd");
            var data = await GetAsync<List<DailyRevenueSummary>>(
                $"/api/v1/internal/orders/revenue-by-date/instructor/{instructorId}?from={fromStr}&to={toStr}");
            return ApiResponse<List<DailyRevenueSummary>>.SuccessResponse(data, "OK");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get instructor revenue from Sale Service. InstructorId={InstructorId}", instructorId);
            return ApiResponse<List<DailyRevenueSummary>>.FailureResponse(ex.Message);
        }
    }
}
