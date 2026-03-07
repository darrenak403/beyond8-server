using Beyond8.Catalog.Domain.Repositories.Interfaces;
using Beyond8.Common.Events.Identity;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Catalog.Application.Consumers.Identity;

public class UserUpdatedEventConsumer(
    ILogger<UserUpdatedEventConsumer> logger,
    IUnitOfWork unitOfWork) : IConsumer<UserUpdatedEvent>
{
    public async Task Consume(ConsumeContext<UserUpdatedEvent> context)
    {
        var message = context.Message;

        try
        {
            logger.LogInformation("Consuming user updated event for user {UserId}", message.UserId);

            var courses = await unitOfWork.CourseRepository.GetAllAsync(c => c.InstructorId == message.UserId);

            if (courses == null || courses.Count == 0)
            {
                logger.LogWarning("No courses found for user {UserId}", message.UserId);
                return;
            }

            if (!string.IsNullOrEmpty(message.FullName))
            {
                foreach (var course in courses)
                {
                    course.InstructorName = message.FullName;
                    await unitOfWork.CourseRepository.UpdateAsync(course.Id, course);
                }
            }

            await unitOfWork.SaveChangesAsync();
            logger.LogInformation("User updated event for user {UserId} consumed successfully", message.UserId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error consuming user updated event for user {UserId}", message.UserId);
            throw;
        }
    }
}