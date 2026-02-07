using Beyond8.Analytic.Domain.Repositories.Interfaces;
using Beyond8.Common.Events.Learning;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Analytic.Application.Consumers.Learning;

public class CourseCompletedEventConsumer(
    ILogger<CourseCompletedEventConsumer> logger,
    IUnitOfWork unitOfWork) : IConsumer<CourseCompletedEvent>
{
    public async Task Consume(ConsumeContext<CourseCompletedEvent> context)
    {
        var message = context.Message;

        var stats = await unitOfWork.AggCourseStatsRepository.GetByCourseIdAsync(message.CourseId);
        if (stats != null)
        {
            stats.TotalCompletedStudents++;
            stats.CompletionRate = stats.TotalStudents > 0
                ? Math.Round((decimal)stats.TotalCompletedStudents / stats.TotalStudents * 100, 2)
                : 0;
            await unitOfWork.AggCourseStatsRepository.UpdateAsync(stats.Id, stats);
        }

        var overview = await unitOfWork.AggSystemOverviewRepository.GetOrCreateCurrentAsync();
        overview.TotalCompletedEnrollments++;
        if (overview.TotalEnrollments > 0)
        {
            overview.AvgCourseCompletionRate = Math.Round(
                (decimal)overview.TotalCompletedEnrollments / overview.TotalEnrollments * 100, 2);
        }

        await unitOfWork.SaveChangesAsync();

        logger.LogInformation("Course completed in analytics: {CourseId} by user {UserId}",
            message.CourseId, message.UserId);
    }
}
