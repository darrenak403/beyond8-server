using Beyond8.Common.Events.Identity;
using Beyond8.Integration.Application.Mappings.NotificationMappings;
using Beyond8.Integration.Application.Services.Interfaces;
using Beyond8.Integration.Domain.Enums;
using Beyond8.Integration.Domain.Repositories.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Integration.Application.Consumers.Identity
{
    public class OtpEmailConsumer(
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        ILogger<OtpEmailConsumer> logger
    ) : IConsumer<OtpEmailEvent>
    {
        public async Task Consume(ConsumeContext<OtpEmailEvent> context)
        {
            var message = context.Message;
            var status = NotificationStatus.Failed;

            try
            {
                logger.LogInformation("Consuming OTP email event for {Email} with purpose {Purpose}",
                    message.ToEmail, message.Purpose);

                try
                {
                    var success = await emailService.SendOtpEmailAsync(
                        message.ToEmail,
                        message.ToName,
                        message.OtpCode,
                        message.Purpose
                    );

                    if (success)
                    {
                        status = NotificationStatus.Delivered;
                        logger.LogInformation("Successfully sent OTP email to {Email} with OTP code {OtpCode}", message.ToEmail, message.OtpCode);
                    }
                    else
                    {
                        logger.LogError("Failed to send OTP email to {Email}", message.ToEmail);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error sending OTP email to {Email}", message.ToEmail);
                }

                try
                {
                    await unitOfWork.NotificationRepository.AddAsync(message.OtpEmailEventToNotification(status));
                    await unitOfWork.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to save notification for OTP email to {Email}", message.ToEmail);
                }

                logger.LogInformation("OTP email event for {Email} with purpose {Purpose} consumed successfully",
                    message.ToEmail, message.Purpose);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error consuming OTP email event for {Email}", message.ToEmail);
                throw;
            }
        }
    }
}
