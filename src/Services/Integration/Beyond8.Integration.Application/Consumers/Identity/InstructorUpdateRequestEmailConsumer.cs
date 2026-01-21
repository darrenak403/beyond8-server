using Beyond8.Common.Events.Identity;
using Beyond8.Integration.Application.Services.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Integration.Application.Consumers.Identity;

public class InstructorUpdateRequestEmailConsumer(
    IEmailService emailService,
    ILogger<InstructorUpdateRequestEmailConsumer> logger
) : IConsumer<InstructorUpdateRequestEmailEvent>
{
    public async Task Consume(ConsumeContext<InstructorUpdateRequestEmailEvent> context)
    {
        var message = context.Message;

        try
        {
            logger.LogInformation("Consuming instructor update request email event for {Email}",
                message.ToEmail);

            var success = await emailService.SendInstructorUpdateRequestEmailAsync(
                message.ToEmail,
                message.InstructorName,
                message.UpdateNotes
            );

            if (success)
                logger.LogInformation("Successfully sent instructor update request email to {Email}", message.ToEmail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error consuming instructor update request email event for {Email}", message.ToEmail);
            throw;
        }
    }
}
