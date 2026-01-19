using System;
using System.ComponentModel.DataAnnotations;
using Beyond8.Identity.Domain.Enums;

namespace Beyond8.Identity.Application.Dtos.Users;

public class UpdateUserRequest
{
    public string FullName { get; set; } = null!;
    public string? AvatarUrl { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Timezone { get; set; } = null!;
    public string Locale { get; set; } = null!;
}
