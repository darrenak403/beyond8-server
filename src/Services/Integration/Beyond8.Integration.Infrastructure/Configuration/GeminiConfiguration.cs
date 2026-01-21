namespace Beyond8.Integration.Infrastructure.Configuration;

public class GeminiConfiguration
{
    public const string SectionName = "Gemini";

    public string ApiKey { get; set; } = string.Empty;
    public string ApiEndpoint { get; set; } = "https://generativelanguage.googleapis.com/v1beta";
    public string DefaultModel { get; set; } = "gemini-2.0-flash";
    public int MaxRetries { get; set; } = 3;
    public int TimeoutSeconds { get; set; } = 30;
    public int RequestsPerMinute { get; set; } = 60;
    public int RequestsPerDay { get; set; } = 1500;
    public int MaxConcurrentRequests { get; set; } = 5;
    public GeminiDefaultParameters DefaultParameters { get; set; } = new();
    public GeminiPricing Pricing { get; set; } = new();
}

public class GeminiDefaultParameters
{
    public int MaxTokens { get; set; } = 8192;
    public decimal Temperature { get; set; } = 0.7m;
    public decimal TopP { get; set; } = 0.95m;
    public int TopK { get; set; } = 40;
}

public class GeminiPricing
{
    public decimal InputCostPer1MTokens { get; set; } = 0.075m;
    public decimal OutputCostPer1MTokens { get; set; } = 0.30m;
}
