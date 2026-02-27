using Beyond8.Common.Events.Catalog;
using Beyond8.Integration.Application.Mappings.NotificationMappings;
using Beyond8.Integration.Application.Services.Interfaces;
using Beyond8.Integration.Domain.Enums;
using Beyond8.Integration.Domain.Repositories.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Integration.Application.Consumers.Catalog;

public class CourseRejectedEventConsumer(
    ILogger<CourseRejectedEventConsumer> logger,
    IEmailService emailService,
    IUnitOfWork unitOfWork) : IConsumer<CourseRejectedEvent>
{
    public async Task Consume(ConsumeContext<CourseRejectedEvent> context)
    {
        var message = context.Message;
        var status = NotificationStatus.Failed;

        try
        {
            logger.LogInformation(
                "Consuming course rejected event: CourseId={CourseId}, CourseName={CourseName}",
                message.CourseId, message.CourseName);

            try
            {
                var success = await emailService.SendCourseRejectedEmailAsync(
                    message.InstructorEmail,
                    message.InstructorName,
                    message.CourseName,
                    message.Reason ?? "Khóa học chưa đáp ứng đủ tiêu chuẩn chất lượng của Beyond8."
                );

                if (success)
                {
                    status = NotificationStatus.Delivered;
                    logger.LogInformation(
                        "Successfully sent course rejected email to {Email} for course {CourseName}",
                        message.InstructorEmail, message.CourseName);
                }
                else
                {
                    logger.LogError(
                        "Failed to send course rejected email to {Email} for course {CourseName}",
                        message.InstructorEmail, message.CourseName);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Error sending course rejected email: CourseId={CourseId}, Email={Email}",
                    message.CourseId, message.InstructorEmail);
            }

            try
            {
                await unitOfWork.NotificationRepository.AddAsync(message.CourseRejectedEventToNotification(status));
                await unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "Failed to save notification for course rejected event: CourseId={CourseId}, Email={Email}",
                    message.CourseId, message.InstructorEmail);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error consuming course rejected event: CourseId={CourseId}, Error={Error}",
                message.CourseId, ex.Message);
            throw;
        }
    }
}