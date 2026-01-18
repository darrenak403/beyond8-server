using System;
using Beyond8.Identity.Application.Dtos.Users;
using Beyond8.Identity.Domain.Entities;
using Beyond8.Identity.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Beyond8.Identity.Application.Mappings.UserMappings;

public static class CreateUserMappings
{
    public static User ToEntity(this CreateUserRequest request, PasswordHasher<User> passwordHasher)
    {
        return new User
        {
            Email = request.Email,
            PasswordHash = passwordHasher.HashPassword(new User(), request.Password),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = Guid.Empty,
            Roles = [UserRole.Student],
            FullName = request.Email.Split('@')[0]
        };
    }
}
