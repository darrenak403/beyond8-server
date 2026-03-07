using Beyond8.Analytic.Domain.Repositories.Interfaces;
using Beyond8.Common.Events.Learning;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Analytic.Application.Consumers.Learning;

public class CourseRatingUpdatedEventConsumer(
    ILogger<CourseRatingUpdatedEventConsumer> logger,
    IUnitOfWork unitOfWork) : IConsumer<CourseRatingUpdatedEvent>
{
    public async Task Consume(ConsumeContext<CourseRatingUpdatedEvent> context)
    {
        var message = context.Message;

        var stats = await unitOfWork.AggCourseStatsRepository.GetByCourseIdAsync(message.CourseId);
        if (stats != null)
        {
            stats.AvgRating = message.CourseAvgRating;
            stats.TotalReviews = message.CourseTotalReviews;
            stats.TotalRatings = message.CourseTotalReviews;
            await unitOfWork.AggCourseStatsRepository.UpdateAsync(stats.Id, stats);
        }

        var instructorRevenue = await unitOfWork.AggInstructorRevenueRepository.GetByInstructorIdAsync(message.InstructorId);
        if (instructorRevenue != null)
        {
            instructorRevenue.AvgCourseRating = message.InstructorAvgRating;
            var allCourses = await unitOfWork.AggCourseStatsRepository.GetByInstructorIdAsync(message.InstructorId);
            instructorRevenue.TotalReviews = allCourses.Sum(c => c.TotalReviews);
            await unitOfWork.AggInstructorRevenueRepository.UpdateAsync(instructorRevenue.Id, instructorRevenue);
        }

        var overview = await unitOfWork.AggSystemOverviewRepository.GetOrCreateCurrentAsync();
        overview.AvgCourseRating = message.CourseAvgRating;
        overview.TotalReviews++;

        await unitOfWork.SaveChangesAsync();

        logger.LogInformation("Course rating updated in analytics: {CourseId}, avg={AvgRating}",
            message.CourseId, message.CourseAvgRating);
    }
}
