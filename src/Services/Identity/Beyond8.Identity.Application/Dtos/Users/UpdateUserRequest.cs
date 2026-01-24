namespace Beyond8.Identity.Application.Dtos.Users;

public class UpdateUserRequest
{
    public string? FullName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Specialization { get; set; } = null;
    public string? Address { get; set; } = null;
    public string? Bio { get; set; } = null;
    public string? Timezone { get; set; }
    public string? Locale { get; set; }
}
