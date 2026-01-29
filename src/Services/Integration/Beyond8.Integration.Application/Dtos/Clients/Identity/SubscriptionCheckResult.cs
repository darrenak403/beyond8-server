namespace Beyond8.Integration.Application.Dtos.Clients.Identity
{
    public class SubscriptionCheckResult
    {
        public bool IsAllowed { get; init; }
        public string Message { get; init; } = string.Empty;
        public DateTime? RequestLimitedEndsAt { get; init; }
        public object? Metadata { get; init; }
    }
}
