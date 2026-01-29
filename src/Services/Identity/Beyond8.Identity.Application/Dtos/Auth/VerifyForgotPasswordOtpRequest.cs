namespace Beyond8.Identity.Application.Dtos.Auth;

public class VerifyForgotPasswordOtpRequest
{
    public string Email { get; set; } = string.Empty;
    public string OtpCode { get; set; } = string.Empty;
}
