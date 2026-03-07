namespace Beyond8.Analytic.Application.Dtos.AiUsage;

public class AiUsageModelSummaryResponse
{
    public string Model { get; set; } = string.Empty;
    public int Provider { get; set; }
    public int TotalInputTokens { get; set; }
    public int TotalOutputTokens { get; set; }
    public int TotalTokens { get; set; }
    public decimal TotalInputCost { get; set; }
    public decimal TotalOutputCost { get; set; }
    public decimal TotalCost { get; set; }
    public int UsageCount { get; set; }
}
