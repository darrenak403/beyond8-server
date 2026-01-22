using Beyond8.Common.Events.Identity;
using Beyond8.Integration.Application.Mappings.NotificationMappings;
using Beyond8.Integration.Application.Services.Interfaces;
using Beyond8.Integration.Domain.Entities;
using Beyond8.Integration.Domain.Enums;
using Beyond8.Integration.Domain.Repositories.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Integration.Application.Consumers.Identity;

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
                await unitOfWork.NotificationRepository.AddAsync(message.OtpEmailEventToNotification(NotificationStatus.Delivered));
                logger.LogInformation("Successfully sent OTP email to {Email}", message.ToEmail);
            }
            else
            {
                await unitOfWork.NotificationRepository.AddAsync(message.OtpEmailEventToNotification(NotificationStatus.Failed));
                logger.LogError("Failed to send OTP email to {Email}", message.ToEmail);
            }
            await unitOfWork.SaveChangesAsync();
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
