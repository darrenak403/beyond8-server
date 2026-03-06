using Beyond8.Analytic.Domain.Repositories.Interfaces;
using Beyond8.Common.Events.Learning;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Analytic.Application.Consumers.Learning;

public class CourseEnrollmentCountChangedEventConsumer(
    ILogger<CourseEnrollmentCountChangedEventConsumer> logger,
    IUnitOfWork unitOfWork) : IConsumer<CourseEnrollmentCountChangedEvent>
{
    public async Task Consume(ConsumeContext<CourseEnrollmentCountChangedEvent> context)
    {
        var message = context.Message;

        var stats = await unitOfWork.AggCourseStatsRepository.GetByCourseIdAsync(message.CourseId);
        if (stats != null)
        {
            stats.TotalStudents = message.TotalStudents;
            await unitOfWork.AggCourseStatsRepository.UpdateAsync(stats.Id, stats);
        }

        if (message.InstructorId.HasValue && message.InstructorId.Value != Guid.Empty)
        {
            var instructorId = message.InstructorId.Value;
            var instructorRevenue = await unitOfWork.AggInstructorRevenueRepository.GetByInstructorIdAsync(instructorId);
            if (instructorRevenue != null)
            {
                var allCourses = await unitOfWork.AggCourseStatsRepository.GetByInstructorIdAsync(instructorId);
                instructorRevenue.TotalStudents = allCourses.Sum(c => c.TotalStudents);
                await unitOfWork.AggInstructorRevenueRepository.UpdateAsync(instructorRevenue.Id, instructorRevenue);
            }
        }

        var overview = await unitOfWork.AggSystemOverviewRepository.GetOrCreateCurrentAsync();
        overview.TotalEnrollments += message.Delta ?? 0;

        if (message.Delta.HasValue && message.Delta.Value > 0)
        {
            var now = DateTime.UtcNow;
            var yearMonth = $"{now.Year:D4}-{now.Month:D2}";
            var monthly = await unitOfWork.AggSystemOverviewMonthlyRepository
                .GetOrCreateForMonthAsync(yearMonth, now.Year, now.Month);
            monthly.NewEnrollments += message.Delta.Value;

            var dateKey = $"{now.Year:D4}-{now.Month:D2}-{now.Day:D2}";
            var daily = await unitOfWork.AggSystemOverviewDailyRepository
                .GetOrCreateForDateAsync(dateKey, now.Year, now.Month, now.Day);
            daily.NewEnrollments += message.Delta.Value;
        }

        await unitOfWork.SaveChangesAsync();

        logger.LogInformation("Enrollment count updated in analytics: Course {CourseId} = {TotalStudents}",
            message.CourseId, message.TotalStudents);
    }
}
