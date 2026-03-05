namespace Beyond8.Integration.Infrastructure.Configuration
{
    public class HuggingFaceSettings
    {
        public const string SectionName = "HuggingFace";

        public string ApiEndpoint { get; set; } = "https://router.huggingface.co/hf-inference/models";
        public string ApiKey { get; set; } = string.Empty;
        public string DefaultModel { get; set; } = "sentence-transformers/all-MiniLM-L6-v2";
        public int MaxRetries { get; set; } = 3;
        public int TimeoutSeconds { get; set; } = 30;
        /// <summary>Giá theo Inference API (USD per 1M tokens). Embedding chỉ tính input.</summary>
        public HuggingFacePricing Pricing { get; set; } = new();
    }

    public class HuggingFacePricing
    {
        public decimal InputCostPer1MTokens { get; set; } = 0.02m;
    }
}
