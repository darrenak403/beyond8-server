using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Beyond8.Common.Security;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public Guid UserId => Guid.Parse(Claim(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());
    public string? Email => Claim(ClaimTypes.Email);

    public string? Claim(string claimType)
    {
        return _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
    }
    public bool IsInRole(string role)
    {
        return _httpContextAccessor.HttpContext?.User.IsInRole(role) ?? false;
    }
    public bool IsInAnyRole(params string[] roles)
    {
        return roles.Any(role => IsInRole(role));
    }
}
