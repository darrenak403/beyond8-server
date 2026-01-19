using System;

namespace Beyond8.Identity.Application.Dtos.Users;

public class UpdateAvatarRequest
{
    public string AvatarUrl { get; set; } = null!;
}
