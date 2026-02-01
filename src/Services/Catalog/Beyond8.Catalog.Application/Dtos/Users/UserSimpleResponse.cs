namespace Beyond8.Catalog.Application.Dtos.Users;

public class UserSimpleResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public DateTime? DateOfBirth { get; set; }
    public string? AvatarUrl { get; set; }
}
