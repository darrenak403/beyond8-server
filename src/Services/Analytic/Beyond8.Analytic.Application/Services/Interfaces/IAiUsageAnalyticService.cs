using Beyond8.Analytic.Application.Dtos.AiUsage;
using Beyond8.Common.Utilities;

namespace Beyond8.Analytic.Application.Services.Interfaces;

public interface IAiUsageAnalyticService
{
    Task<ApiResponse<List<AiUsageChartByDateResponse>>> GetAiUsageChartAsync(AiUsageChartRequest request);
}
