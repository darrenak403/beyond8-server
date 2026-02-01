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

        try
        {
            logger.LogInformation(
                "Consuming course rejected event: CourseId={CourseId}, CourseName={CourseName}",
                message.CourseId, message.CourseName);

            var success = await emailService.SendCourseRejectedEmailAsync(
                message.InstructorEmail,
                message.InstructorName,
                message.CourseName,
                message.Reason ?? "Khóa học chưa đáp ứng đủ tiêu chuẩn chất lượng của Beyond8."
            );

            if (success)
            {
                logger.LogInformation(
                    "Successfully sent course rejected email to {Email} for course {CourseName}",
                    message.InstructorEmail, message.CourseName);
            }

            try
            {
                var status = success ? NotificationStatus.Delivered : NotificationStatus.Failed;
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