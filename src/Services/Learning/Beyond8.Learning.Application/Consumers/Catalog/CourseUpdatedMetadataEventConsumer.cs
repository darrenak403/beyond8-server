using Beyond8.Common.Events.Catalog;
using Beyond8.Learning.Domain.Repositories.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Learning.Application.Consumers.Catalog
{
    public class CourseUpdatedMetadataEventConsumer(
        ILogger<CourseUpdatedMetadataEventConsumer> logger,
        IUnitOfWork unitOfWork) : IConsumer<CourseUpdatedMetadataEvent>
    {
        public async Task Consume(ConsumeContext<CourseUpdatedMetadataEvent> context)
        {
            var message = context.Message;

            try
            {
                logger.LogInformation("Consuming course updated metadata event: {CourseId}", message.CourseId);

                var enrollment = await unitOfWork.EnrollmentRepository.FindOneAsync(e => e.CourseId == message.CourseId);
                if (enrollment == null)
                {
                    logger.LogWarning("Course not found for updated metadata: {CourseId}", message.CourseId);
                    return;
                }

                enrollment.CourseTitle = message.Title;
                enrollment.Slug = message.Slug;
                enrollment.CourseThumbnailUrl = message.ThumbnailUrl;
                await unitOfWork.EnrollmentRepository.UpdateAsync(enrollment.Id, enrollment);
                await unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error consuming course updated metadata event: {CourseId}", message.CourseId);
            }
        }
    }
}