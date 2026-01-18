using System;
using System.ComponentModel.DataAnnotations;
using Beyond8.Identity.Domain.Enums;

namespace Beyond8.Identity.Application.Dtos.Users;

public class CreateUserRequest
{
    [Required(ErrorMessage = "Email là bắt buộc.")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    [MaxLength(255, ErrorMessage = "Email không được vượt quá 255 ký tự.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
    [MinLength(8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự.")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Họ tên là bắt buộc.")]
    [MaxLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự.")]
    public string FullName { get; set; } = null!;

    // [MaxLength(500, ErrorMessage = "URL avatar không được vượt quá 500 ký tự.")]
    // public string? AvatarUrl { get; set; }

    [MaxLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự.")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
    public string? PhoneNumber { get; set; }

    public List<UserRole> Roles { get; set; } = [UserRole.Student];
}
