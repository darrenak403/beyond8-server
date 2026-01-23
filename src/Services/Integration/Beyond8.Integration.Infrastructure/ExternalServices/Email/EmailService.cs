using Beyond8.Integration.Application.Services.Interfaces;
using Beyond8.Integration.Infrastructure.Configurations;
using Beyond8.Integration.Infrastructure.ExternalServices.Email.Templates;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Resend;

namespace Beyond8.Integration.Infrastructure.ExternalServices.Email;

public class EmailService(
    ILogger<EmailService> logger,
    IOptions<ResendSettings> resendOptions,
    IResend resend
) : IEmailService
{
    private readonly ResendSettings _resendConfig = resendOptions.Value;
    private readonly string _fromEmail = resendOptions.Value.FromEmail;
    private readonly string _fromName = resendOptions.Value.FromName;

    public async Task<bool> SendOtpEmailAsync(string toEmail, string toName, string otpCode, string purpose)
    {
        try
        {
            var htmlContent = EmailTemplates.GetOtpEmailTemplate(otpCode, purpose);

            var message = new EmailMessage
            {
                From = $"{_fromName} <{_fromEmail}>",
                To = new[] { toEmail },
                Subject = $"[Beyond8] Mã xác thực OTP - {purpose}",
                HtmlBody = htmlContent
            };

            var response = await resend.EmailSendAsync(message);

            logger.LogInformation("OTP email sent successfully to {Email} for {Purpose}",
                toEmail, purpose);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending OTP email to {Email}: {Message}", toEmail, ex.Message);
            throw;
        }
    }

    public async Task<bool> SendInstructorApprovalEmailAsync(string toEmail, string instructorName, string profileUrl)
    {
        try
        {
            var htmlContent = EmailTemplates.GetInstructorApprovalEmailTemplate(instructorName, profileUrl);

            var message = new EmailMessage
            {
                From = $"{_fromName} <{_fromEmail}>",
                To = new[] { toEmail },
                Subject = "🎉 Chúc mừng! Bạn đã trở thành Giảng viên Beyond8",
                HtmlBody = htmlContent
            };

            var response = await resend.EmailSendAsync(message);

            logger.LogInformation("Instructor approval email sent successfully to {Email}",
                toEmail);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending instructor approval email to {Email}: {Message}", toEmail, ex.Message);
            throw;
        }
    }

    public async Task<bool> SendInstructorRejectionEmailAsync(string toEmail, string instructorName, string reason)
    {
        try
        {
            var htmlContent = EmailTemplates.GetInstructorRejectionEmailTemplate(instructorName, reason);

            var message = new EmailMessage
            {
                From = $"{_fromName} <{_fromEmail}>",
                To = new[] { toEmail },
                Subject = "Thông báo về đơn đăng ký Giảng viên Beyond8",
                HtmlBody = htmlContent
            };

            var response = await resend.EmailSendAsync(message);

            logger.LogInformation("Instructor rejection email sent successfully to {Email}",
                toEmail);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending instructor rejection email to {Email}: {Message}", toEmail, ex.Message);
            throw;
        }
    }

    public async Task<bool> SendInstructorUpdateRequestEmailAsync(string toEmail, string instructorName, string updateNotes)
    {
        try
        {
            var htmlContent = EmailTemplates.GetInstructorUpdateRequestEmailTemplate(instructorName, updateNotes);

            var message = new EmailMessage
            {
                From = $"{_fromName} <{_fromEmail}>",
                To = new[] { toEmail },
                Subject = "📝 Yêu cầu cập nhật hồ sơ Giảng viên Beyond8",
                HtmlBody = htmlContent
            };

            var response = await resend.EmailSendAsync(message);

            logger.LogInformation("Instructor update request email sent successfully to {Email}",
                toEmail);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending instructor update request email to {Email}: {Message}", toEmail, ex.Message);
            throw;
        }
    }
}
