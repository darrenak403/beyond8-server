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

            try
            {
                logger.LogInformation("Consuming OTP email event for {Email} with purpose {Purpose}",
                    message.ToEmail, message.Purpose);

                var success = await emailService.SendOtpEmailAsync(
                    message.ToEmail,
                    message.ToName,
                    message.OtpCode,
                    message.Purpose
                );

                if (success)
                {
                    logger.LogInformation("Successfully sent OTP email to {Email}", message.ToEmail);

                    try
                    {
                        await unitOfWork.NotificationRepository.AddAsync(message.OtpEmailEventToNotification(NotificationStatus.Delivered));
                        await unitOfWork.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Failed to save notification for OTP email to {Email}, but email was sent successfully", message.ToEmail);
                    }
                }
                else
                {
                    logger.LogError("Failed to send OTP email to {Email}", message.ToEmail);

                    try
                    {
                        await unitOfWork.NotificationRepository.AddAsync(message.OtpEmailEventToNotification(NotificationStatus.Failed));
                        await unitOfWork.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Failed to save notification for OTP email to {Email}", message.ToEmail);
                    }
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
