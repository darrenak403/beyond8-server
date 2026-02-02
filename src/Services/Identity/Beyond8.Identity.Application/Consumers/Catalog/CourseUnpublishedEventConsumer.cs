using Beyond8.Common.Events.Catalog;
using Beyond8.Identity.Domain.Repositories.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Identity.Application.Consumers.Catalog;

public class CourseUnpublishedEventConsumer(
    ILogger<CourseUnpublishedEventConsumer> logger,
    IUnitOfWork unitOfWork) : IConsumer<CourseUnpublishedEvent>
{
    public async Task Consume(ConsumeContext<CourseUnpublishedEvent> context)
    {
        var message = context.Message;

        try
        {
            logger.LogInformation(
                "Consuming course unpublished event: CourseId={CourseId}, InstructorId={InstructorId}",
                message.CourseId, message.InstructorId);

            var instructorProfile = await unitOfWork.InstructorProfileRepository
                .FindOneAsync(p => p.UserId == message.InstructorId);

            if (instructorProfile == null)
            {
                logger.LogWarning(
                    "Instructor profile not found for user {InstructorId} when processing course unpublished event",
                    message.InstructorId);
                return;
            }

            // Only decrement if TotalCourses > 0 to avoid negative values
            if (instructorProfile.TotalCourses > 0)
            {
                instructorProfile.TotalCourses--;
                await unitOfWork.InstructorProfileRepository.UpdateAsync(instructorProfile.Id, instructorProfile);
                await unitOfWork.SaveChangesAsync();

                logger.LogInformation(
                    "Successfully decremented TotalCourses for instructor {InstructorId}. New total: {TotalCourses}",
                    message.InstructorId, instructorProfile.TotalCourses);
            }
            else
            {
                logger.LogWarning(
                    "TotalCourses for instructor {InstructorId} is already 0, cannot decrement",
                    message.InstructorId);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error consuming course unpublished event: CourseId={CourseId}, InstructorId={InstructorId}, Error={Error}",
                message.CourseId, message.InstructorId, ex.Message);
            throw;
        }
    }
}
