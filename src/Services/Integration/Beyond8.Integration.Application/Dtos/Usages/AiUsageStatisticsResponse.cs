namespace Beyond8.Integration.Application.Dtos.Usages
{
    public class AiUsageStatisticsResponse
    {
        public int TotalUsage { get; set; }
        public decimal TotalCost { get; set; }
        public int TotalTokens { get; set; }
        public int TotalInputTokens { get; set; }
        public int TotalOutputTokens { get; set; }
        public decimal TotalInputCost { get; set; }
        public decimal TotalOutputCost { get; set; }
    }
}
