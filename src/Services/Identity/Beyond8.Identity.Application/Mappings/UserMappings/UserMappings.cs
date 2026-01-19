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

    public static User ToUserEntity(this CreateUserRequest request, Guid createdBy)
    {
        var user = new User
        {
            Email = request.Email,
            FullName = request.FullName,
            AvatarUrl = request.AvatarUrl,
            PhoneNumber = request.PhoneNumber,
            Timezone = request.Timezone,
            Locale = request.Locale,
            Roles = request.Roles ?? [UserRole.Student],
            IsActive = true,
            IsEmailVerified = false,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        return user;
    }
    public static void UpdateFromRequest(this User user, UpdateUserRequest request, Guid updatedBy)
    {
        if (!string.IsNullOrEmpty(request.Email))
            user.Email = request.Email;

        if (!string.IsNullOrEmpty(request.FullName))
            user.FullName = request.FullName;

        if (!string.IsNullOrEmpty(request.AvatarUrl))
            user.AvatarUrl = request.AvatarUrl;

        if (!string.IsNullOrEmpty(request.PhoneNumber))
            user.PhoneNumber = request.PhoneNumber;

        if (!string.IsNullOrEmpty(request.Timezone))
            user.Timezone = request.Timezone;

        if (!string.IsNullOrEmpty(request.Locale))
            user.Locale = request.Locale;

        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = updatedBy;
    }
}
