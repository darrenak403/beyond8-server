using Beyond8.Common.Events.Learning;
using Beyond8.Integration.Application.Dtos.Notifications;
using Beyond8.Integration.Application.Mappings.NotificationMappings;
using Beyond8.Integration.Application.Services.Interfaces;
using Beyond8.Integration.Domain.Enums;
using Beyond8.Integration.Domain.Repositories.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Integration.Application.Consumers.Learning;

public class CourseCompletedEventConsumer(
    ILogger<CourseCompletedEventConsumer> logger,
    IUnitOfWork unitOfWork,
    INotificationService notificationService,
    IEmailService emailService) : IConsumer<CourseCompletedEvent>
{
    public async Task Consume(ConsumeContext<CourseCompletedEvent> context)
    {
        var message = context.Message;
        var inAppStatus = NotificationStatus.Failed;
        var emailStatus = NotificationStatus.Failed;

        try
        {
            logger.LogInformation(
                "Consuming course completed event: EnrollmentId={EnrollmentId}, UserId={UserId}, CourseId={CourseId}, CertificateId={CertificateId}",
                message.EnrollmentId, message.UserId, message.CourseId, message.CertificateId);

            var data = new DataInfor
            {
                Title = "Chúc mừng hoàn thành khóa học",
                Message = $"Bạn đã hoàn thành khóa học \"{message.CourseTitle}\". Bạn có thể xem chứng chỉ trong hồ sơ của mình.",
                Metadata = new
                {
                    enrollmentId = message.EnrollmentId,
                    courseId = message.CourseId,
                    certificateId = message.CertificateId
                }
            };

            try
            {
                await notificationService.SendToUserAsync(message.UserId.ToString(), "CourseCompleted", data);
                inAppStatus = NotificationStatus.Delivered;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "Failed to send in-app course completed notification: EnrollmentId={EnrollmentId}, UserId={UserId}",
                    message.EnrollmentId, message.UserId);
            }

            if (message.CertificateId.HasValue && !string.IsNullOrEmpty(message.UserEmail))
            {
                try
                {
                    var userName = message.UserFullName ?? "Bạn";
                    var success = await emailService.SendCourseCompletedEmailAsync(
                        message.UserEmail,
                        userName,
                        message.CourseTitle);

                    if (success)
                    {
                        emailStatus = NotificationStatus.Delivered;
                        logger.LogInformation(
                            "Course completed email sent to {Email} for course {CourseTitle}",
                            message.UserEmail, message.CourseTitle);
                    }
                    else
                    {
                        logger.LogError(
                            "Failed to send course completed email to {Email} for course {CourseTitle}",
                            message.UserEmail, message.CourseTitle);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex,
                        "Error sending course completed email: EnrollmentId={EnrollmentId}, UserId={UserId}, Email={Email}",
                        message.EnrollmentId, message.UserId, message.UserEmail);
                }
            }

            try
            {
                var status = inAppStatus == NotificationStatus.Delivered || emailStatus == NotificationStatus.Delivered
                    ? NotificationStatus.Delivered
                    : NotificationStatus.Failed;

                await unitOfWork.NotificationRepository.AddAsync(message.CourseCompletedEventToNotification(status, data));
                await unitOfWork.SaveChangesAsync();
                logger.LogInformation(
                    "Successfully saved notification for course completed event: EnrollmentId={EnrollmentId}, UserId={UserId}",
                    message.EnrollmentId, message.UserId);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "Failed to save notification for course completed event: EnrollmentId={EnrollmentId}, UserId={UserId}",
                    message.EnrollmentId, message.UserId);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error consuming course completed event: EnrollmentId={EnrollmentId}, UserId={UserId}, Error={Error}",
                message.EnrollmentId, message.UserId, ex.Message);
            throw;
        }
    }
}
