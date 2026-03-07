using Beyond8.Common.Events.Sale;
using Beyond8.Learning.Application.Services.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Learning.Application.Consumers.Sale;

public class OrderCompletedEventConsumer(
    ILogger<OrderCompletedEventConsumer> logger,
    IEnrollmentService enrollmentService) : IConsumer<OrderCompletedEvent>
{
    public async Task Consume(ConsumeContext<OrderCompletedEvent> context)
    {
        var message = context.Message;

        try
        {
            logger.LogInformation("Processing enrollment for order {OrderId}, user {UserId}, courses: {CourseCount}",
                message.OrderId, message.UserId, message.CourseIds.Count);

            // Enroll user in all purchased courses
            var result = await enrollmentService.EnrollPaidCoursesAsync(message.UserId, message.CourseIds, message.OrderId);

            if (result.IsSuccess)
            {
                logger.LogInformation("Successfully enrolled user {UserId} in {CourseCount} courses from order {OrderId}",
                    message.UserId, message.CourseIds.Count, message.OrderId);
            }
            else
            {
                logger.LogError("Failed to enroll user {UserId} in courses from order {OrderId}: {Message}",
                    message.UserId, message.OrderId, result.Message);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing enrollment for order {OrderId}, user {UserId}",
                message.OrderId, message.UserId);
            throw; // Let MassTransit handle retry policy
        }
    }
}