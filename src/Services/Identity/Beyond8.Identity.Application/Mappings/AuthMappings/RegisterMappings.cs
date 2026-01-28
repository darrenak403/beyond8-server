using Beyond8.Identity.Application.Dtos.Auth;
using Beyond8.Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Beyond8.Identity.Application.Mappings.AuthMappings
{
    public static class RegisterMappings
    {
        public static User ToEntity(this RegisterRequest request, PasswordHasher<User> passwordHasher)
        {
            var normalizedEmail = request.Email.ToLower().Trim();
            return new User
            {
                Email = normalizedEmail,
                PasswordHash = passwordHasher.HashPassword(new User(), request.Password),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Guid.Empty,
                FullName = normalizedEmail.Split('@')[0],
                IsEmailVerified = false,
            };
        }
    }
}
