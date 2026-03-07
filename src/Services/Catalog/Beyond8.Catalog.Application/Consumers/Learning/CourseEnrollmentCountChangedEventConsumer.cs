using Beyond8.Catalog.Domain.Repositories.Interfaces;
using Beyond8.Common.Events.Learning;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Catalog.Application.Consumers.Learning;

public class CourseEnrollmentCountChangedEventConsumer(
    ILogger<CourseEnrollmentCountChangedEventConsumer> logger,
    IUnitOfWork unitOfWork) : IConsumer<CourseEnrollmentCountChangedEvent>
{
    public async Task Consume(ConsumeContext<CourseEnrollmentCountChangedEvent> context)
    {
        var message = context.Message;

        var course = await unitOfWork.CourseRepository.FindOneAsync(c => c.Id == message.CourseId && c.IsActive);
        if (course == null)
        {
            logger.LogWarning("Course not found for enrollment count update: {CourseId}", message.CourseId);
            return;
        }

        course.TotalStudents = message.TotalStudents;
        await unitOfWork.CourseRepository.UpdateAsync(course.Id, course);
        await unitOfWork.SaveChangesAsync();

        logger.LogInformation("Course {CourseId} TotalStudents updated to {TotalStudents} via event",
            message.CourseId, message.TotalStudents);
    }
}
