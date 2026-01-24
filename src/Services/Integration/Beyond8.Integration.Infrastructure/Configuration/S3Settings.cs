namespace Beyond8.Integration.Infrastructure.Configuration;

public class S3Settings
{
    public const string SectionName = "AWS:S3";

    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public string CloudFrontUrl { get; set; } = string.Empty;
}
