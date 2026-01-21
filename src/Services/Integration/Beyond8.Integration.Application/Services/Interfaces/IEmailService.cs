namespace Beyond8.Integration.Application.Services.Interfaces;

public interface IEmailService
{
    Task<bool> SendOtpEmailAsync(string toEmail, string toName, string otpCode, string purpose);

    Task<bool> SendInstructorApprovalEmailAsync(string toEmail, string instructorName, string profileUrl);

    Task<bool> SendInstructorRejectionEmailAsync(string toEmail, string instructorName, string reason);

    Task<bool> SendInstructorUpdateRequestEmailAsync(string toEmail, string instructorName, string updateNotes);
}
