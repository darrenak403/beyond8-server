namespace Beyond8.Analytic.Application.Dtos.AiUsage;

public class AiUsageChartByDateResponse
{
    public DateOnly SnapshotDate { get; set; }
    public List<AiUsageModelSummaryResponse> Models { get; set; } = [];
}
