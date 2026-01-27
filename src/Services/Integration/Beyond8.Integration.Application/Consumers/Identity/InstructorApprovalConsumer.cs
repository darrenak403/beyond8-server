using Beyond8.Common.Events.Identity;
using Beyond8.Integration.Application.Dtos.Notifications;
using Beyond8.Integration.Application.Mappings.NotificationMappings;
using Beyond8.Integration.Application.Services.Interfaces;
using Beyond8.Integration.Domain.Enums;
using Beyond8.Integration.Domain.Repositories.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Integration.Application.Consumers.Identity;

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
                    // Save email notification
                    await unitOfWork.NotificationRepository.AddAsync(message.InstructorApprovalEventToNotification(NotificationStatus.Delivered));

                    // Save re-login notification
                    await unitOfWork.NotificationRepository.AddAsync(message.ReLoginNotificationToNotification(NotificationStatus.Delivered));
                    await unitOfWork.SaveChangesAsync();

                    var data = new DataInfor
                    {
                        Title = "Yêu cầu đăng nhập lại",
                        Message = "Tài khoản của bạn đã được duyệt thành công. Vui lòng đăng xuất và đăng nhập lại để cập nhật quyền truy cập.",
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
                    logger.LogInformation("Successfully sent real-time re-login notification to user {UserId}", message.UserId);
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
                    await unitOfWork.NotificationRepository.AddAsync(message.InstructorApprovalEventToNotification(NotificationStatus.Failed));
                    await unitOfWork.NotificationRepository.AddAsync(message.ReLoginNotificationToNotification(NotificationStatus.Failed));
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
