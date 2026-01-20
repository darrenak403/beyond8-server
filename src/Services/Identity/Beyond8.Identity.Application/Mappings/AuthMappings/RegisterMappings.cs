using Beyond8.Identity.Application.Dtos.Auth;
using Beyond8.Identity.Domain.Entities;
using Beyond8.Identity.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Beyond8.Identity.Application.Mappings.AuthMappings;

public static class RegisterMappings
{
    public static User ToEntity(this RegisterRequest request, PasswordHasher<User> passwordHasher)
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
