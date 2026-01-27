namespace Beyond8.Identity.Application.Dtos.Tokens
{
    public class TokenClaims
    {
        public Guid UserId { get; set; } = Guid.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = [];
        public string SubscriptionTier { get; set; } = "FREE";
        public DateTime? SubscriptionExpiresAt { get; set; }
    }
}
