using Beyond8.Common.Events.Identity;
using Beyond8.Integration.Application.Services.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Integration.Application.Consumers.Identity;

public class InstructorRejectionEmailConsumer(
    IEmailService emailService,
    ILogger<InstructorRejectionEmailConsumer> logger
) : IConsumer<InstructorRejectionEmailEvent>
{
    public async Task Consume(ConsumeContext<InstructorRejectionEmailEvent> context)
    {
        var message = context.Message;

        try
        {
            logger.LogInformation("Consuming instructor rejection email event for {Email}",
                message.ToEmail);

            var success = await emailService.SendInstructorRejectionEmailAsync(
                message.ToEmail,
                message.InstructorName,
                message.Reason
            );

            if (success)
                logger.LogInformation("Successfully sent instructor rejection email to {Email}", message.ToEmail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error consuming instructor rejection email event for {Email}", message.ToEmail);
            throw;
        }
    }
}
