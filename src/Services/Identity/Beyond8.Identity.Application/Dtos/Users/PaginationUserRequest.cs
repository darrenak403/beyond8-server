using Beyond8.Common.Utilities;

namespace Beyond8.Identity.Application.Dtos.Users
{
    public class PaginationUserRequest : PaginationRequest
    {
        public string? Email { get; set; } = string.Empty;
        public string? FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; } = string.Empty;
        public string? Specialization { get; set; } = string.Empty;
        public string? Address { get; set; } = string.Empty;
        public bool? IsEmailVerified { get; set; }
        public string? Role { get; set; }
    }
}
