using System.ComponentModel.DataAnnotations;
using Beyond8.Common.Data.Base;
using Beyond8.Identity.Domain.Enums;

namespace Beyond8.Identity.Domain.Entities
{
    public class User : BaseEntity
    {
        [Required]
        [MaxLength(255)]
        public string Email { get; set; } = null!;
        [Required]
        public string PasswordHash { get; set; } = null!;
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = null!;
        public DateTime? DateOfBirth { get; set; }
        [MaxLength(500)]
        public string? AvatarUrl { get; set; }
        [MaxLength(500)]
        public string? CoverUrl { get; set; }
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }
        [MaxLength(200)]
        public string? Specialization { get; set; }
        [MaxLength(500)]
        public string? Address { get; set; }
        [MaxLength(1000)]
        public string? Bio { get; set; }
        public bool IsEmailVerified { get; set; } = false;
        public DateTime? LastLoginAt { get; set; }
        public int LoginAttempts { get; set; } = 0;
        public DateTime? LockedUntil { get; set; }
        [MaxLength(50)]
        public string Timezone { get; set; } = "Asia/Ho_Chi_Minh";
        [MaxLength(10)]
        public string Locale { get; set; } = "vi-VN";
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiresAt { get; set; }
        public bool? IsRevoked { get; set; } = false;
        public DateTime? RevokedAt { get; set; }
        public UserStatus Status { get; set; } = UserStatus.Active;
        public virtual InstructorProfile? InstructorProfile { get; set; }
    }
}