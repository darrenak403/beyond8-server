using Beyond8.Analytic.Application.Dtos.AiUsage;
using Beyond8.Analytic.Application.Mappings;
using Beyond8.Analytic.Application.Services.Interfaces;
using Beyond8.Analytic.Domain.Repositories.Interfaces;
using Beyond8.Common.Utilities;
using Microsoft.Extensions.Logging;

namespace Beyond8.Analytic.Application.Services.Implements;

public class AiUsageAnalyticService(
    IUnitOfWork unitOfWork,
    ILogger<AiUsageAnalyticService> logger) : IAiUsageAnalyticService
{
    private static readonly int[] AllowedPeriodMonths = [1, 3, 6, 9, 12];

    public async Task<ApiResponse<List<AiUsageDailyChartItemResponse>>> GetAiUsageChartAsync(AiUsageChartRequest request)
    {
        try
        {
            DateOnly startDate;
            DateOnly endDate;

            if (request.PeriodMonths.HasValue)
            {
                var months = request.PeriodMonths.Value;
                if (!AllowedPeriodMonths.Contains(months))
                {
                    return ApiResponse<List<AiUsageDailyChartItemResponse>>.FailureResponse(
                        "PeriodMonths chỉ chấp nhận: 1, 3, 6, 9, 12.");
                }
                endDate = DateOnly.FromDateTime(DateTime.UtcNow);
                startDate = endDate.AddMonths(-months);
            }
            else if (request.StartDate.HasValue && request.EndDate.HasValue)
            {
                startDate = request.StartDate.Value;
                endDate = request.EndDate.Value;
                if (startDate > endDate)
                {
                    return ApiResponse<List<AiUsageDailyChartItemResponse>>.FailureResponse(
                        "StartDate phải nhỏ hơn hoặc bằng EndDate.");
                }
            }
            else
            {
                return ApiResponse<List<AiUsageDailyChartItemResponse>>.FailureResponse(
                    "Cung cấp PeriodMonths (1,3,6,9,12) hoặc StartDate và EndDate.");
            }

            var items = await unitOfWork.AggAiUsageDailyRepository.GetByDateRangeAsync(startDate, endDate);
            var response = items.Select(e => e.ToChartItemResponse()).ToList();

            logger.LogInformation("AiUsage chart: {Start} - {End}, {Count} records", startDate, endDate, response.Count);
            return ApiResponse<List<AiUsageDailyChartItemResponse>>.SuccessResponse(
                response, "Lấy dữ liệu AI usage thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting AI usage chart");
            return ApiResponse<List<AiUsageDailyChartItemResponse>>.FailureResponse(
                "Đã xảy ra lỗi khi lấy dữ liệu AI usage.");
        }
    }
}
