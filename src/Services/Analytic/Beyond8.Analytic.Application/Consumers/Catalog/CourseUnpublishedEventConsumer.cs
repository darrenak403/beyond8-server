using Beyond8.Analytic.Domain.Repositories.Interfaces;
using Beyond8.Common.Events.Catalog;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Analytic.Application.Consumers.Catalog;

public class CourseUnpublishedEventConsumer(
    ILogger<CourseUnpublishedEventConsumer> logger,
    IUnitOfWork unitOfWork) : IConsumer<CourseUnpublishedEvent>
{
    public async Task Consume(ConsumeContext<CourseUnpublishedEvent> context)
    {
        var message = context.Message;

        var overview = await unitOfWork.AggSystemOverviewRepository.GetOrCreateCurrentAsync();
        if (overview.TotalPublishedCourses > 0)
            overview.TotalPublishedCourses--;

        await unitOfWork.SaveChangesAsync();

        logger.LogInformation("Course unpublished in analytics: {CourseId}, TotalPublishedCourses={Count}",
            message.CourseId, overview.TotalPublishedCourses);
    }
}
