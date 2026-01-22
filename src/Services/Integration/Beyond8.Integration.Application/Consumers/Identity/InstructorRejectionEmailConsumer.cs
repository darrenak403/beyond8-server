using Beyond8.Common.Events.Identity;
using Beyond8.Integration.Application.Mappings.NotificationMappings;
using Beyond8.Integration.Application.Services.Interfaces;
using Beyond8.Integration.Domain.Enums;
using Beyond8.Integration.Domain.Repositories.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Integration.Application.Consumers.Identity;

public class InstructorRejectionEmailConsumer(
    IEmailService emailService,
    IUnitOfWork unitOfWork,
    ILogger<InstructorRejectionEmailConsumer> logger
) : IConsumer<InstructorRejectionEmailEvent>
{
    public async Task Consume(ConsumeContext<InstructorRejectionEmailEvent> context)
    {
        var message = context.Message;

        try
        {
            logger.LogInformation("Consuming instructor rejection email event for {Email}", message.ToEmail);

            var success = await emailService.SendInstructorRejectionEmailAsync(
                message.ToEmail,
                message.InstructorName,
                message.Reason
            );

            if (success)
                logger.LogInformation("Successfully sent instructor rejection email to {Email}", message.ToEmail);

            var status = success ? NotificationStatus.Delivered : NotificationStatus.Failed;
            await unitOfWork.NotificationRepository.AddAsync(message.InstructorRejectionEmailEventToNotification(status));
            await unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error consuming instructor rejection email event for {Email}", message.ToEmail);
            throw;
        }
    }
}
