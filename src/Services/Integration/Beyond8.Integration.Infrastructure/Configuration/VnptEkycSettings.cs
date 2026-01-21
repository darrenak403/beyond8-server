namespace Beyond8.Integration.Infrastructure.Configuration
{
    public class VnptEkycSettings
    {
        public const string SectionName = "VnptEkyc";
        public string BaseUrl { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public string TokenId { get; set; } = string.Empty;
        public string TokenKey { get; set; } = string.Empty;
        public int TimeoutSeconds { get; set; } = 30;
    }
}