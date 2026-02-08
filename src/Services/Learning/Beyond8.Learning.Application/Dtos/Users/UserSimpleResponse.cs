namespace Beyond8.Learning.Application.Dtos.Users;

public class UserSimpleResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
}
