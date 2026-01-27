using Beyond8.Common.Events.Identity;
using Beyond8.Common.Utilities;
using Beyond8.Integration.Application.Dtos.Notifications;
using Beyond8.Integration.Application.Mappings.NotificationMappings;
using Beyond8.Integration.Application.Services.Interfaces;
using Beyond8.Integration.Domain.Enums;
using Beyond8.Integration.Domain.Repositories.Interfaces;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Beyond8.Integration.Application.Consumers.Identity;

public class InstructorProfileSubmittedConsumer(
    INotificationService notificationService,
    IUnitOfWork unitOfWork,
    ILogger<InstructorProfileSubmittedConsumer> logger,
    IConfiguration configuration
) : IConsumer<InstructorProfileSubmittedEvent>
{
    public async Task Consume(ConsumeContext<InstructorProfileSubmittedEvent> context)
    {
        var message = context.Message;

        try
        {
            logger.LogInformation("Consuming instructor application submitted event for {Email}, profile {ProfileId}",
                message.Email, message.ProfileId);

            var frontendUrl = configuration.GetValue<string>("FrontendUrl") ?? "http://localhost:5173";
            var data = new DataInfor
            {
                Title = "Hồ sơ giảng viên",
                Message = $"Hồ sơ giảng viên đã được gửi bởi email {message.Email}.",
                Metadata = new
                {
                    userId = message.UserId,
                }
            };

            await notificationService.SendToGroupAsync($"{Role.Admin}Group", "InstructorApplicationSubmitted", data);
            await notificationService.SendToGroupAsync($"{Role.Staff}Group", "InstructorApplicationSubmitted", data);

            logger.LogInformation("Successfully sent instructor application submitted SignalR notification to Admin and Staff");

            // Save notifications to database for Admin and Staff
            try
            {
                var adminNotification = message.InstructorProfileSubmittedEventToNotification(NotificationTarget.AllAdmin, NotificationStatus.Delivered);
                var staffNotification = message.InstructorProfileSubmittedEventToNotification(NotificationTarget.AllStaff, NotificationStatus.Delivered);

                await unitOfWork.NotificationRepository.AddAsync(adminNotification);
                await unitOfWork.NotificationRepository.AddAsync(staffNotification);
                await unitOfWork.SaveChangesAsync();

                logger.LogInformation("Successfully saved instructor application submitted notifications to database");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to save notification for instructor application submitted event, but SignalR notification was sent successfully");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error consuming instructor application submitted event for {Email}", message.Email);
            throw;
        }
    }
}
