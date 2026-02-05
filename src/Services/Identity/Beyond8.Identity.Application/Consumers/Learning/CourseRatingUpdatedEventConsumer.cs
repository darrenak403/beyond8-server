using Beyond8.Common.Events.Learning;
using Beyond8.Identity.Domain.Repositories.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Identity.Application.Consumers.Learning;

public class CourseRatingUpdatedEventConsumer(
    ILogger<CourseRatingUpdatedEventConsumer> logger,
    IUnitOfWork unitOfWork) : IConsumer<CourseRatingUpdatedEvent>
{
    public async Task Consume(ConsumeContext<CourseRatingUpdatedEvent> context)
    {
        var message = context.Message;

        var instructorProfile = await unitOfWork.InstructorProfileRepository
            .FindOneAsync(p => p.UserId == message.InstructorId);

        if (instructorProfile == null)
        {
            logger.LogWarning("Instructor profile not found for rating update: {InstructorId}", message.InstructorId);
            return;
        }

        instructorProfile.AvgRating = message.InstructorAvgRating;
        await unitOfWork.InstructorProfileRepository.UpdateAsync(instructorProfile.Id, instructorProfile);
        await unitOfWork.SaveChangesAsync();

        logger.LogInformation("Instructor {InstructorId} AvgRating updated to {AvgRating} via event",
            message.InstructorId, message.InstructorAvgRating);
    }
}
