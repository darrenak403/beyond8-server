using System;
using System.ComponentModel.DataAnnotations;
using Beyond8.Identity.Domain.Enums;

namespace Beyond8.Identity.Application.Dtos.Users;

public class UpdateUserRequest
{
    public string? Email { get; set; }
    public string? FullName { get; set; }
    // public string? AvatarUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Timezone { get; set; }
    public string? Locale { get; set; }
}
