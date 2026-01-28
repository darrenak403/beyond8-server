using Beyond8.Common.Events.Identity;
using Beyond8.Integration.Application.Mappings.NotificationMappings;
using Beyond8.Integration.Application.Services.Interfaces;
using Beyond8.Integration.Domain.Enums;
using Beyond8.Integration.Domain.Repositories.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Integration.Application.Consumers.Identity
{
    public class InstructorUpdateRequestEmailConsumer(
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        ILogger<InstructorUpdateRequestEmailConsumer> logger
    ) : IConsumer<InstructorUpdateRequestEvent>
    {
        public async Task Consume(ConsumeContext<InstructorUpdateRequestEvent> context)
        {
            var message = context.Message;

            try
            {
                logger.LogInformation("Consuming instructor update request email event for {Email}", message.ToEmail);

                var success = await emailService.SendInstructorUpdateRequestEmailAsync(
                    message.ToEmail,
                    message.InstructorName,
                    message.UpdateNotes
                );

                if (success)
                {
                    logger.LogInformation("Successfully sent instructor update request email to {Email}", message.ToEmail);

                    try
                    {
                        await unitOfWork.NotificationRepository.AddAsync(message.InstructorUpdateRequestEventToNotification(NotificationStatus.Delivered));
                        await unitOfWork.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Failed to save notification for instructor update request email to {Email}, but email was sent successfully", message.ToEmail);
                    }
                }
                else
                {
                    logger.LogError("Failed to send instructor update request email to {Email}", message.ToEmail);

                    try
                    {
                        await unitOfWork.NotificationRepository.AddAsync(message.InstructorUpdateRequestEventToNotification(NotificationStatus.Failed));
                        await unitOfWork.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Failed to save notification for instructor update request email to {Email}", message.ToEmail);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error consuming instructor update request email event for {Email}", message.ToEmail);
                throw;
            }
        }
    }
}
