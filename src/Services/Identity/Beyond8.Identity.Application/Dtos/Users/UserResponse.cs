using Beyond8.Identity.Domain.Enums;

namespace Beyond8.Identity.Application.Dtos.Users
{
    public class UserResponse
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public List<string> Roles { get; set; } = [];
        public string FullName { get; set; } = null!;
        public DateTime? DateOfBirth { get; set; }
        public string? AvatarUrl { get; set; }
        public string? CoverUrl { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Specialization { get; set; }
        public string? Address { get; set; }
        public string? Bio { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsEmailVerified { get; set; } = false;
        public DateTime? LastLoginAt { get; set; }
        public string Timezone { get; set; } = "Asia/Ho_Chi_Minh";
        public string Locale { get; set; } = "vi-VN";
        public UserStatus Status { get; set; } = UserStatus.Active;
    }

    public class UserSimpleResponse
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public DateTime? DateOfBirth { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
