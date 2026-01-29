namespace Beyond8.Integration.Infrastructure.Configuration
{
    public class BedrockSettings
    {
        public const string SectionName = "AWS:Bedrock";

        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string Region { get; set; } = "ap-southeast-1";
        public string DefaultModel { get; set; } = "anthropic.claude-3-5-sonnet-20241022-v2:0";
        public int MaxRetries { get; set; } = 3;
        public int TimeoutSeconds { get; set; } = 30;
        public int RequestsPerMinute { get; set; } = 60;
        public int RequestsPerDay { get; set; } = 1500;
        public int MaxConcurrentRequests { get; set; } = 5;
        public BedrockDefaultParameters DefaultParameters { get; set; } = new();
        public BedrockPricing Pricing { get; set; } = new();
    }

    public class BedrockDefaultParameters
    {
        public int MaxTokens { get; set; } = 8192;
        public decimal Temperature { get; set; } = 0.7m;
        public decimal TopP { get; set; } = 0.95m;
    }

    public class BedrockPricing
    {
        public decimal InputCostPer1MTokens { get; set; } = 3.00m;
        public decimal OutputCostPer1MTokens { get; set; } = 15.00m;
    }
}
