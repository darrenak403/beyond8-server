using System;
using System.ComponentModel.DataAnnotations;

namespace Beyond8.Identity.Application.Dtos.Auth;

public class ResetPasswordRequest
{
    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mã OTP không được để trống")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP phải gồm 6 chữ số")]
    public string OtpCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
    [MinLength(8, ErrorMessage = "Mật khẩu tối thiểu 8 ký tự")]
    public string NewPassword { get; set; } = string.Empty;

    [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}