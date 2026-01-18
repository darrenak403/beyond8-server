using System;
using System.ComponentModel.DataAnnotations;
using Beyond8.Identity.Domain.Enums;

namespace Beyond8.Identity.Application.Dtos.Users;

public class UpdateUserRequest
{
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    [MaxLength(255, ErrorMessage = "Email không được vượt quá 255 ký tự.")]
    public string? Email { get; set; }

    [MaxLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự.")]
    public string? FullName { get; set; }

    // [MaxLength(500, ErrorMessage = "URL avatar không được vượt quá 500 ký tự.")]
    // public string? AvatarUrl { get; set; }

    [MaxLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự.")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
    public string? PhoneNumber { get; set; }

    [MaxLength(50, ErrorMessage = "Timezone không được vượt quá 50 ký tự.")]
    public string? Timezone { get; set; }

    [MaxLength(10, ErrorMessage = "Locale không được vượt quá 10 ký tự.")]
    public string? Locale { get; set; }
}
