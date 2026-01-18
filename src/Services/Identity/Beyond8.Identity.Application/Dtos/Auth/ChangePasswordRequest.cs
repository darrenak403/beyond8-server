using System;
using System.ComponentModel.DataAnnotations;

namespace Beyond8.Identity.Application.Dtos.Auth;

public class ChangePasswordRequest
{
    [Required(ErrorMessage = "Mật khẩu cũ không được để trống")]
    [MinLength(8, ErrorMessage = "Mật khẩu cũ tối thiểu 8 ký tự")]
    [MaxLength(100)]
    public string OldPassword { get; set; } = string.Empty;
    [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
    [MinLength(8, ErrorMessage = "Mật khẩu mới tối thiểu 8 ký tự")]
    [MaxLength(100)]
    public string NewPassword { get; set; } = string.Empty;
}
