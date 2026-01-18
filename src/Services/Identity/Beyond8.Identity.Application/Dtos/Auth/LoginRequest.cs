using System.ComponentModel.DataAnnotations;

namespace Beyond8.Identity.Application.Dtos.Auth;

public class LoginRequest
{
    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password không được để trống")]
    [MinLength(8, ErrorMessage = "Password tối thiểu 8 ký tự")]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;
}
