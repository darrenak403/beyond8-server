using Beyond8.Common.Events.Identity;
using Beyond8.Integration.Application.Mappings.NotificationMappings;
using Beyond8.Integration.Application.Services.Interfaces;
using Beyond8.Integration.Domain.Enums;
using Beyond8.Integration.Domain.Repositories.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Integration.Application.Consumers.Identity;

public class InstructorApprovalEmailConsumer(
    IEmailService emailService,
    IUnitOfWork unitOfWork,
    ILogger<InstructorApprovalEmailConsumer> logger
) : IConsumer<InstructorApprovalEmailEvent>
{
    public async Task Consume(ConsumeContext<InstructorApprovalEmailEvent> context)
    {
        var message = context.Message;

        try
        {
            logger.LogInformation("Consuming instructor approval email event for {Email}", message.ToEmail);

            var success = await emailService.SendInstructorApprovalEmailAsync(
                message.ToEmail,
                message.InstructorName,
                message.ProfileUrl
            );

            if (success)
            {
                logger.LogInformation("Successfully sent instructor approval email to {Email}", message.ToEmail);

                try
                {
                    await unitOfWork.NotificationRepository.AddAsync(message.InstructorApprovalEmailEventToNotification(NotificationStatus.Delivered));
                    await unitOfWork.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to save notification for instructor approval email to {Email}, but email was sent successfully", message.ToEmail);
                }
            }
            else
            {
                logger.LogError("Failed to send instructor approval email to {Email}", message.ToEmail);

                try
                {
                    await unitOfWork.NotificationRepository.AddAsync(message.InstructorApprovalEmailEventToNotification(NotificationStatus.Failed));
                    await unitOfWork.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to save notification for instructor approval email to {Email}", message.ToEmail);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error consuming instructor approval email event for {Email}", message.ToEmail);
            throw;
        }
    }
}
