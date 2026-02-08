using Beyond8.Analytic.Domain.Repositories.Interfaces;
using Beyond8.Common.Events.Catalog;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Analytic.Application.Consumers.Catalog;

public class CourseUpdatedMetadataEventConsumer(
    ILogger<CourseUpdatedMetadataEventConsumer> logger,
    IUnitOfWork unitOfWork) : IConsumer<CourseUpdatedMetadataEvent>
{
    public async Task Consume(ConsumeContext<CourseUpdatedMetadataEvent> context)
    {
        var message = context.Message;

        var stats = await unitOfWork.AggCourseStatsRepository.GetByCourseIdAsync(message.CourseId);
        if (stats == null)
        {
            logger.LogWarning("AggCourseStats not found for metadata update: {CourseId}", message.CourseId);
            return;
        }

        stats.CourseTitle = message.Title;
        await unitOfWork.AggCourseStatsRepository.UpdateAsync(stats.Id, stats);
        await unitOfWork.SaveChangesAsync();

        logger.LogInformation("Course metadata updated in analytics: {CourseId}", message.CourseId);
    }
}
