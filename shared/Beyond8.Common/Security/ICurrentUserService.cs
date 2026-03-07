namespace Beyond8.Common.Security
{
    public interface ICurrentUserService
    {
        Guid UserId { get; }
        string? Email { get; }
        string? Claim(string claimType);
        bool IsInRole(string role);
        bool IsInAnyRole(params string[] roles);
        bool IsAuthenticated { get; }
    }
}
