namespace Beyond8.Integration.Application.Dtos.AiIntegration;

public class AiUsageStatisticsResponse
{
    public int TotalUsage { get; set; }
    public int TotalCost { get; set; }
    public int TotalTokens { get; set; }
    public int TotalInputTokens { get; set; }
    public int TotalOutputTokens { get; set; }
    public int TotalInputCost { get; set; }
    public int TotalOutputCost { get; set; }
}
