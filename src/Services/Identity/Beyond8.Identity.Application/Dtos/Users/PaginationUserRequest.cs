using System;
using Beyond8.Common.Utilities;
using Beyond8.Identity.Domain.Enums;

namespace Beyond8.Identity.Application.Dtos.Users;

public class PaginationUserRequest : PaginationRequest
{
    public string? Email { get; set; } = string.Empty;
    public string? FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; } = string.Empty;
    public bool? IsEmailVerified { get; set; }
    public UserRole? Role { get; set; }
}
