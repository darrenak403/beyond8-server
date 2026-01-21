using Beyond8.Identity.Application.Dtos.Tokens;
using Beyond8.Identity.Application.Dtos.Users;
using Beyond8.Identity.Domain.Entities;
using Beyond8.Identity.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Beyond8.Identity.Application.Mappings.AuthMappings;

public static class UserMappings
{
    public static UserResponse ToUserResponse(this User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            Roles = user.Roles,
            FullName = user.FullName,
            AvatarUrl = user.AvatarUrl,
            CoverUrl = user.CoverUrl,
            PhoneNumber = user.PhoneNumber,
            Status = user.Status,
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
            AvatarUrl = user.AvatarUrl,
            CoverUrl = user.CoverUrl,
        };
    }

    public static User ToUserEntity(this CreateUserRequest request)
    {
        var user = new User
        {
            Email = request.Email,
            FullName = request.FullName,
            AvatarUrl = request.AvatarUrl,
            CoverUrl = request.CoverUrl,
            PhoneNumber = request.PhoneNumber,
            Timezone = request.Timezone,
            Locale = request.Locale,
            Roles = request.Roles ?? [UserRole.Student],
            Status = UserStatus.Active,
            IsEmailVerified = false
        };

        return user;
    }
    public static void UpdateFromRequest(this User user, UpdateUserRequest request, Guid updatedBy)
    {
        if (!string.IsNullOrEmpty(request.FullName))
            user.FullName = request.FullName;

        if (!string.IsNullOrEmpty(request.PhoneNumber))
            user.PhoneNumber = request.PhoneNumber;

        if (!string.IsNullOrEmpty(request.Timezone))
            user.Timezone = request.Timezone;

        if (!string.IsNullOrEmpty(request.Locale))
            user.Locale = request.Locale;
    }
}
