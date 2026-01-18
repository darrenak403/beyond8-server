using System;
using System.ComponentModel.DataAnnotations;

namespace Beyond8.Identity.Application.Dtos.Auth;

public class VerifyOtpRequest
{
    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mã OTP không được để trống")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP phải bao gồm 6 ký tự")]
    [RegularExpression(@"^\d+$", ErrorMessage = "Mã OTP chỉ được chứa số")]
    public string OtpCode { get; set; } = string.Empty;
}
