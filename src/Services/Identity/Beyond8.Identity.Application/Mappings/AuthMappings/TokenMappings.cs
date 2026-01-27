using Beyond8.Identity.Application.Dtos.Tokens;
using Beyond8.Identity.Domain.Entities;

namespace Beyond8.Identity.Application.Mappings.AuthMappings;

public static class TokenMappings
{
    public static TokenClaims ToTokenClaims(this User user)
    {
        return new TokenClaims
        {
            UserId = user.Id,
            Email = user.Email,
            UserName = user.FullName,
            Roles = [
                .. user.UserRoles
                    .Where(ur =>
                        ur.Role.Code == "ROLE_INSTRUCTOR"
                        || (ur.Role.Code != "ROLE_INSTRUCTOR" && ur.RevokedAt == null)
                    )
                    .Select(ur => ur.Role.Code)
            ]
        };
    }
}
