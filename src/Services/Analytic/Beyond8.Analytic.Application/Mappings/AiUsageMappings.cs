using Beyond8.Analytic.Application.Dtos.AiUsage;
using Beyond8.Analytic.Domain.Entities;

namespace Beyond8.Analytic.Application.Mappings;

public static class AiUsageMappings
{
    public static AiUsageDailyChartItemResponse ToChartItemResponse(this AggAiUsageDaily entity) => new()
    {
        SnapshotDate = entity.SnapshotDate,
        Model = entity.Model,
        Provider = entity.Provider,
        TotalInputTokens = entity.TotalInputTokens,
        TotalOutputTokens = entity.TotalOutputTokens,
        TotalTokens = entity.TotalTokens,
        TotalInputCost = entity.TotalInputCost,
        TotalOutputCost = entity.TotalOutputCost,
        TotalCost = entity.TotalCost,
        UsageCount = entity.UsageCount
    };

    public static AiUsageModelSummaryResponse ToModelSummaryResponse(this AggAiUsageDaily entity) => new()
    {
        Model = entity.Model,
        Provider = entity.Provider,
        TotalInputTokens = entity.TotalInputTokens,
        TotalOutputTokens = entity.TotalOutputTokens,
        TotalTokens = entity.TotalTokens,
        TotalInputCost = entity.TotalInputCost,
        TotalOutputCost = entity.TotalOutputCost,
        TotalCost = entity.TotalCost,
        UsageCount = entity.UsageCount
    };
}
