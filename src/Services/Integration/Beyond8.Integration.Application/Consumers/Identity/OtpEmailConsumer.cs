using Beyond8.Common.Events.Identity;
using Beyond8.Integration.Application.Services.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Integration.Application.Consumers.Identity;

public class OtpEmailConsumer(
    IEmailService emailService,
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
                logger.LogInformation("Successfully sent OTP email to {Email}", message.ToEmail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error consuming OTP email event for {Email}", message.ToEmail);
            throw; // MassTransit will handle retry
        }
    }
}
