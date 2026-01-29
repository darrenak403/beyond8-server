namespace Beyond8.Integration.Infrastructure.Configurations;

public class ResendSettings
{
    public const string SectionName = "Email:Resend";

    public string ApiKey { get; set; } = string.Empty;
    public string FromEmail { get; set; } = "onboarding@resend.dev";
    public string FromName { get; set; } = "Beyond8";
}
