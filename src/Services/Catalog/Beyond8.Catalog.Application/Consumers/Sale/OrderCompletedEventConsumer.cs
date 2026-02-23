using Beyond8.Catalog.Domain.Repositories.Interfaces;
using Beyond8.Common.Events.Sale;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Catalog.Application.Consumers.Sale;

public class OrderCompletedEventConsumer(
    ILogger<OrderCompletedEventConsumer> logger,
    IUnitOfWork unitOfWork) : IConsumer<OrderCompletedEvent>
{
    public async Task Consume(ConsumeContext<OrderCompletedEvent> context)
    {
        var message = context.Message;

        try
        {
            logger.LogInformation("OrderCompletedEvent received: OrderId={OrderId}, Courses={Count}",
                message.OrderId, message.CourseIds.Count);

            foreach (var courseId in message.CourseIds.Distinct())
            {
                var course = await unitOfWork.CourseRepository.FindOneAsync(c => c.Id == courseId && c.IsActive);
                if (course == null)
                {
                    logger.LogWarning("Course not found for increment: {CourseId}", courseId);
                    continue;
                }

                course.TotalStudents = course.TotalStudents + 1;
                course.UpdatedAt = DateTime.UtcNow;

                await unitOfWork.CourseRepository.UpdateAsync(course.Id, course);
            }

            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Incremented TotalStudents for {Count} courses from order {OrderId}",
                message.CourseIds.Count, message.OrderId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling OrderCompletedEvent for OrderId={OrderId}", message.OrderId);
            throw; // let MassTransit retry according to policy
        }
    }
}
