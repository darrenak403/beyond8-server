using Beyond8.Common.Events.Catalog;
using Beyond8.Identity.Domain.Repositories.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Identity.Application.Consumers.Catalog;

public class CoursePublishedEventConsumer(
    ILogger<CoursePublishedEventConsumer> logger,
    IUnitOfWork unitOfWork) : IConsumer<CoursePublishedEvent>
{
    public async Task Consume(ConsumeContext<CoursePublishedEvent> context)
    {
        var message = context.Message;

        try
        {
            logger.LogInformation(
                "Consuming course published event: CourseId={CourseId}, InstructorId={InstructorId}",
                message.CourseId, message.InstructorId);

            var instructorProfile = await unitOfWork.InstructorProfileRepository
                .FindOneAsync(p => p.UserId == message.InstructorId);

            if (instructorProfile == null)
            {
                logger.LogWarning(
                    "Instructor profile not found for user {InstructorId} when processing course published event",
                    message.InstructorId);
                return;
            }

            instructorProfile.TotalCourses++;
            await unitOfWork.InstructorProfileRepository.UpdateAsync(instructorProfile.Id, instructorProfile);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation(
                "Successfully incremented TotalCourses for instructor {InstructorId}. New total: {TotalCourses}",
                message.InstructorId, instructorProfile.TotalCourses);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error consuming course published event: CourseId={CourseId}, InstructorId={InstructorId}, Error={Error}",
                message.CourseId, message.InstructorId, ex.Message);
            throw;
        }
    }
}
