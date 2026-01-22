using Beyond8.Common.Events.Identity;
using Beyond8.Common.Utilities;
using Beyond8.Integration.Application.Services.Interfaces;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Beyond8.Integration.Application.Consumers.Identity;

public class InstructorApplicationSubmittedConsumer(
    INotificationService notificationService,
    ILogger<InstructorApplicationSubmittedConsumer> logger,
    IConfiguration configuration
) : IConsumer<InstructorApplicationSubmittedEvent>
{
    public async Task Consume(ConsumeContext<InstructorApplicationSubmittedEvent> context)
    {
        var message = context.Message;

        try
        {
            logger.LogInformation("Consuming instructor application submitted event for {Email}, profile {ProfileId}",
                message.Email, message.ProfileId);

            var frontendUrl = configuration.GetValue<string>("FrontendUrl") ?? "http://localhost:5173";
            var data = new
            {
                userId = message.UserId,
                profileId = message.ProfileId,
                instructorName = message.InstructorName,
                email = message.Email,
                profileUrl = $"{frontendUrl}/instructor/{message.ProfileId}/admin",
                timestamp = message.Timestamp
            };

            await notificationService.SendToGroupAsync($"{Role.Admin}Group", "InstructorApplicationSubmitted", data);
            await notificationService.SendToGroupAsync($"{Role.Staff}Group", "InstructorApplicationSubmitted", data);

            logger.LogInformation("Successfully sent instructor application submitted SignalR notification to Admin and Staff");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error consuming instructor application submitted event for {Email}", message.Email);
            throw;
        }
    }
}
