using Beyond8.Common.Events.Learning;
using Beyond8.Identity.Domain.Repositories.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Identity.Application.Consumers.Learning;

public class CourseEnrollmentCountChangedEventConsumer(
    ILogger<CourseEnrollmentCountChangedEventConsumer> logger,
    IUnitOfWork unitOfWork) : IConsumer<CourseEnrollmentCountChangedEvent>
{
    public async Task Consume(ConsumeContext<CourseEnrollmentCountChangedEvent> context)
    {
        var message = context.Message;

        if (!message.InstructorId.HasValue || !message.Delta.HasValue)
            return;

        try
        {
            var instructorProfile = await unitOfWork.InstructorProfileRepository
                .FindOneAsync(p => p.UserId == message.InstructorId.Value);

            if (instructorProfile == null)
            {
                logger.LogWarning(
                    "Instructor profile not found for user {InstructorId} when processing enrollment count event",
                    message.InstructorId);
                return;
            }

            instructorProfile.TotalStudents += message.Delta.Value;
            if (instructorProfile.TotalStudents < 0)
                instructorProfile.TotalStudents = 0;

            await unitOfWork.InstructorProfileRepository.UpdateAsync(instructorProfile.Id, instructorProfile);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation(
                "Instructor {InstructorId} TotalStudents updated by {Delta}. New total: {TotalStudents} (CourseId={CourseId})",
                message.InstructorId, message.Delta, instructorProfile.TotalStudents, message.CourseId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error consuming course enrollment count event: CourseId={CourseId}, InstructorId={InstructorId}, Error={Error}",
                message.CourseId, message.InstructorId, ex.Message);
            throw;
        }
    }
}
