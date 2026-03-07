using Beyond8.Common.Events.Catalog;
using Beyond8.Integration.Application.Mappings.NotificationMappings;
using Beyond8.Integration.Application.Services.Interfaces;
using Beyond8.Integration.Domain.Enums;
using Beyond8.Integration.Domain.Repositories.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Integration.Application.Consumers.Catalog;

public class CourseApprovedEventConsumer(
    ILogger<CourseApprovedEventConsumer> logger,
    IEmailService emailService,
    IUnitOfWork unitOfWork) : IConsumer<CourseApprovedEvent>
{
    public async Task Consume(ConsumeContext<CourseApprovedEvent> context)
    {
        var message = context.Message;
        var status = NotificationStatus.Failed;

        try
        {
            logger.LogInformation(
                "Consuming course approved event: CourseId={CourseId}, CourseName={CourseName}",
                message.CourseId, message.CourseName);

            try
            {
                var success = await emailService.SendCourseApprovedEmailAsync(
                    message.InstructorEmail,
                    message.InstructorName,
                    message.CourseName
                );

                if (success)
                {
                    status = NotificationStatus.Delivered;
                    logger.LogInformation(
                        "Successfully sent course approved email to {Email} for course {CourseName}",
                        message.InstructorEmail, message.CourseName);
                }
                else
                {
                    logger.LogError(
                        "Failed to send course approved email to {Email} for course {CourseName}",
                        message.InstructorEmail, message.CourseName);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Error sending course approved email: CourseId={CourseId}, Email={Email}",
                    message.CourseId, message.InstructorEmail);
            }

            try
            {
                await unitOfWork.NotificationRepository.AddAsync(message.CourseApprovedEventToNotification(status));
                await unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "Failed to save notification for course approved event: CourseId={CourseId}, Email={Email}",
                    message.CourseId, message.InstructorEmail);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error consuming course approved event: CourseId={CourseId}, Error={Error}",
                message.CourseId, ex.Message);
            throw;
        }
    }
}
