using Beyond8.Identity.Application.Dtos.Tokens;
using Beyond8.Identity.Application.Dtos.Users;
using Beyond8.Identity.Domain.Entities;
using Beyond8.Identity.Domain.Enums;

namespace Beyond8.Identity.Application.Mappings.AuthMappings;

public static class UserMappings
{
    public static UserResponse ToUserResponse(this User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            PasswordHash = user.PasswordHash,
            Roles = user.Roles,
            FullName = user.FullName,
            AvatarUrl = user.AvatarUrl,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            IsEmailVerified = user.IsEmailVerified,
            LastLoginAt = user.LastLoginAt,
            Timezone = user.Timezone,
            Locale = user.Locale,
        };
    }

    public static UserSimpleResponse ToUserSimpleResponse(this User user)
    {
        return new UserSimpleResponse
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
        };
    }

    public static TokenClaims ToTokenClaims(this User user)
    {
        return new TokenClaims
        {
            UserId = user.Id,
            Email = user.Email,
            UserName = user.FullName,
            Roles = user.Roles,
        };
    }

    public static User ToCreateUserEntity(this CreateUserRequest request)
    {
        return new User
        {
            Email = request.Email,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = Guid.Empty,
            Roles = new List<UserRole> { UserRole.Student },
            FullName = request.Email.Split('@')[0]
        };
    }

}
