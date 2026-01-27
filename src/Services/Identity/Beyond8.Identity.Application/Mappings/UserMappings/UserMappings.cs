using Beyond8.Identity.Application.Dtos.Users;
using Beyond8.Identity.Domain.Entities;
using Beyond8.Identity.Domain.Enums;

namespace Beyond8.Identity.Application.Mappings.AuthMappings
{
    public static class UserMappings
    {
        public static UserResponse ToUserResponse(this User user)
        {
            return new UserResponse
            {
                Id = user.Id,
                Email = user.Email,
                Roles = [.. user.UserRoles
                    .Where(ur => ur.RevokedAt == null)
                    .Select(ur => ur.Role.Code)],
                FullName = user.FullName,
                DateOfBirth = user.DateOfBirth,
                AvatarUrl = user.AvatarUrl,
                CoverUrl = user.CoverUrl,
                PhoneNumber = user.PhoneNumber,
                Specialization = user.Specialization,
                Address = user.Address,
                Bio = user.Bio,
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
                DateOfBirth = user.DateOfBirth
            };
        }

        public static User ToUserEntity(this CreateUserRequest request)
        {
            var user = new User
            {
                Email = request.Email.ToLower().Trim(),
                FullName = request.FullName,
                DateOfBirth = request.DateOfBirth.HasValue
                    ? DateTime.SpecifyKind(request.DateOfBirth.Value.Date, DateTimeKind.Utc)
                    : null,
                AvatarUrl = request.AvatarUrl,
                CoverUrl = request.CoverUrl,
                PhoneNumber = request.PhoneNumber,
                Specialization = request.Specialization,
                Address = request.Address,
                Bio = request.Bio,
                Timezone = request.Timezone,
                Locale = request.Locale,
                UserRoles = [],
                Status = UserStatus.Active,
                IsEmailVerified = true
            };

            return user;
        }
        public static void UpdateFromRequest(this User user, UpdateUserRequest request, Guid updatedBy)
        {
            if (!string.IsNullOrEmpty(request.FullName))
                user.FullName = request.FullName;

            if (request.DateOfBirth.HasValue)
                user.DateOfBirth = DateTime.SpecifyKind(request.DateOfBirth.Value.Date, DateTimeKind.Utc);

            if (!string.IsNullOrEmpty(request.PhoneNumber))
                user.PhoneNumber = request.PhoneNumber;

            if (!string.IsNullOrEmpty(request.Timezone))
                user.Timezone = request.Timezone;

            if (!string.IsNullOrEmpty(request.Locale))
                user.Locale = request.Locale;

            if (!string.IsNullOrEmpty(request.Specialization))
                user.Specialization = request.Specialization;

            if (!string.IsNullOrEmpty(request.Address))
                user.Address = request.Address;

            if (!string.IsNullOrEmpty(request.Bio))
                user.Bio = request.Bio;

        }
    }
}
