using Beyond8.Common.Events.Identity;
using Beyond8.Integration.Application.Dtos.Notifications;
using Beyond8.Integration.Application.Mappings.NotificationMappings;
using Beyond8.Integration.Application.Services.Interfaces;
using Beyond8.Integration.Domain.Enums;
using Beyond8.Integration.Domain.Repositories.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Integration.Application.Consumers.Identity
{
    public class InstructorApprovalConsumer(
        IEmailService emailService,
        INotificationService notificationService,
        IUnitOfWork unitOfWork,
        ILogger<InstructorApprovalConsumer> logger
    ) : IConsumer<InstructorApprovalEvent>
    {
        public async Task Consume(ConsumeContext<InstructorApprovalEvent> context)
        {
            var message = context.Message;
            var emailStatus = NotificationStatus.Failed;
            var reLoginStatus = NotificationStatus.Failed;

            try
            {
                logger.LogInformation("Consuming instructor approval email event for {Email}", message.ToEmail);
                try
                {
                    var emailSent = await emailService.SendInstructorApprovalEmailAsync(
                        message.ToEmail,
                        message.InstructorName,
                        message.ProfileUrl
                    );

                    if (emailSent)
                    {
                        emailStatus = NotificationStatus.Delivered;
                        logger.LogInformation("Successfully sent instructor approval email to {Email}", message.ToEmail);
                    }
                    else
                    {
                        logger.LogError("Failed to send instructor approval email to {Email}", message.ToEmail);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error sending instructor approval email to {Email}", message.ToEmail);
                }

                try
                {
                    var data = new DataInfor
                    {
                        Title = "Đơn giảng viên được duyệt",
                        Message = $"Đơn giảng viên của bạn {message.InstructorName} đã được duyệt thành công. Xem hồ sơ: {message.ProfileUrl}",
                        Metadata = new
                        {
                            RequireReLogin = true
                        }
                    };

                    await notificationService.SendToUserAsync(
                        message.UserId.ToString(),
                        "RequireReLogin",
                        data
                    );

                    reLoginStatus = NotificationStatus.Delivered;
                    logger.LogInformation("Successfully sent real-time re-login notification to user {UserId}", message.UserId);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to send real-time re-login notification to user {UserId}", message.UserId);
                }

                try
                {
                    await unitOfWork.NotificationRepository.AddAsync(message.InstructorApprovalEventToNotification(emailStatus));
                    await unitOfWork.NotificationRepository.AddAsync(message.ReLoginNotificationToNotification(reLoginStatus));
                    await unitOfWork.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to save notification records for instructor approval event of {Email}", message.ToEmail);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error consuming instructor approval email event for {Email}", message.ToEmail);
                throw;
            }
        }
    }
}
