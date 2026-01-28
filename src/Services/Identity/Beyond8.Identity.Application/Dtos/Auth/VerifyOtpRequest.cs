namespace Beyond8.Identity.Application.Dtos.Auth
{
    public class VerifyOtpRequest
    {
        public string Email { get; set; } = string.Empty;
        public string OtpCode { get; set; } = string.Empty;
    }
}
