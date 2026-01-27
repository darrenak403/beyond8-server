namespace Beyond8.Integration.Infrastructure.Configuration
{
    public class QdrantSettings
    {
        public const string SectionName = "Qdrant";

        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 6333;
        public bool UseHttps { get; set; } = false;
        public string? ApiKey { get; set; }
        public int TimeoutSeconds { get; set; } = 30;
        public int VectorDimension { get; set; } = 384;
    }
}
