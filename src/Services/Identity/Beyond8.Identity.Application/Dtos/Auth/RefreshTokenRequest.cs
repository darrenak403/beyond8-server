using System;
using System.ComponentModel.DataAnnotations;

namespace Beyond8.Identity.Application.Dtos.Auth;

public class RefreshTokenRequest
{
    [Required(ErrorMessage = "Refresh token không được để trống")]
    public string RefreshToken { get; set; } = string.Empty;
}
