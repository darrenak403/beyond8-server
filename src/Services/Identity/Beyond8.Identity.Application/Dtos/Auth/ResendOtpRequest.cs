using System;
using System.ComponentModel.DataAnnotations;

namespace Beyond8.Identity.Application.Dtos.Auth;

public class ResendOtpRequest
{
    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;
}

