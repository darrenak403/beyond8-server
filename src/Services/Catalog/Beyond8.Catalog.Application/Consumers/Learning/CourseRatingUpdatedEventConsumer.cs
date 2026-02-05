using Beyond8.Catalog.Domain.Repositories.Interfaces;
using Beyond8.Common.Events.Learning;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Catalog.Application.Consumers.Learning;

public class CourseRatingUpdatedEventConsumer(
    ILogger<CourseRatingUpdatedEventConsumer> logger,
    IUnitOfWork unitOfWork) : IConsumer<CourseRatingUpdatedEvent>
{
    public async Task Consume(ConsumeContext<CourseRatingUpdatedEvent> context)
    {
        var message = context.Message;

        var course = await unitOfWork.CourseRepository.FindOneAsync(c => c.Id == message.CourseId && c.IsActive);
        if (course == null)
        {
            logger.LogWarning("Course not found for rating update: {CourseId}", message.CourseId);
            return;
        }

        course.AvgRating = message.CourseAvgRating;
        course.TotalReviews = message.CourseTotalReviews;
        await unitOfWork.CourseRepository.UpdateAsync(course.Id, course);
        await unitOfWork.SaveChangesAsync();

        logger.LogInformation("Course {CourseId} AvgRating={AvgRating}, TotalReviews={TotalReviews} updated via event",
            message.CourseId, message.CourseAvgRating, message.CourseTotalReviews);
    }
}
