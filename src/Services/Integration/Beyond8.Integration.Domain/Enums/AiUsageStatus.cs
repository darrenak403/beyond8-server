namespace Beyond8.Integration.Domain.Enums
{
    public enum AiUsageStatus
    {
        Success = 0,
        Failed = 1,
        RateLimited = 2,
        InvalidRequest = 3,
        InsufficientQuota = 4,
        Timeout = 5
    }
}
