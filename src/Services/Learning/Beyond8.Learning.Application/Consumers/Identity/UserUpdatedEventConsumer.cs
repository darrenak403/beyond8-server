using Beyond8.Common.Events.Identity;
using Beyond8.Learning.Domain.Repositories.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Learning.Application.Consumers.Identity;

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

            var enrollments = await unitOfWork.EnrollmentRepository.GetAllAsync(e => e.UserId == message.UserId);
            if (enrollments == null || enrollments.Count == 0)
            {
                logger.LogWarning("No enrollments found for user {UserId}", message.UserId);
                return;
            }

            if (!string.IsNullOrEmpty(message.FullName))
            {
                foreach (var enrollment in enrollments)
                {
                    enrollment.InstructorName = message.FullName;
                    await unitOfWork.EnrollmentRepository.UpdateAsync(enrollment.Id, enrollment);
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