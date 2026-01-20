using Beyond8.Identity.Domain.Enums;

namespace Beyond8.Identity.Application.Dtos.Users;

public class CreateUserRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public DateTime? DateOfBirth { get; set; }
    public string? AvatarUrl { get; set; } = null;
    public string? CoverUrl { get; set; } = null;
    public string PhoneNumber { get; set; } = null!;
    public string Timezone { get; set; } = "Asia/Ho_Chi_Minh";
    public string Locale { get; set; } = "vi-VN";
    public List<UserRole> Roles { get; set; } = [UserRole.Student];
}
