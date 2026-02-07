namespace Beyond8.Integration.Application.Services.Interfaces;

public interface IEmailService
{
    Task<bool> SendOtpEmailAsync(string toEmail, string toName, string otpCode, string purpose);

    Task<bool> SendInstructorApprovalEmailAsync(string toEmail, string instructorName, string profileUrl);

    Task<bool> SendInstructorRejectionEmailAsync(string toEmail, string instructorName, string reason);

    Task<bool> SendInstructorUpdateRequestEmailAsync(string toEmail, string instructorName, string updateNotes);

    Task<bool> SendCourseRejectedEmailAsync(string toEmail, string instructorName, string courseName, string reason);

    Task<bool> SendCourseApprovedEmailAsync(string toEmail, string instructorName, string courseName);

    Task<bool> SendCourseCompletedEmailAsync(string toEmail, string userName, string courseName);
}
