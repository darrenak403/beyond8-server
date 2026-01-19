using System;
using System.ComponentModel.DataAnnotations;
using Beyond8.Identity.Domain.Enums;

namespace Beyond8.Identity.Application.Dtos.Users;

public class CreateUserRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string AvatarUrl { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Timezone { get; set; } = null!;
    public string Locale { get; set; } = null!;
    public List<UserRole> Roles { get; set; } = new List<UserRole> { UserRole.Student };
}
