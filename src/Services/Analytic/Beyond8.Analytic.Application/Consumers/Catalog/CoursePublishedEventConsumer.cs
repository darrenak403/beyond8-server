using Beyond8.Analytic.Domain.Entities;
using Beyond8.Analytic.Domain.Repositories.Interfaces;
using Beyond8.Common.Events.Catalog;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Analytic.Application.Consumers.Catalog;

public class CoursePublishedEventConsumer(
    ILogger<CoursePublishedEventConsumer> logger,
    IUnitOfWork unitOfWork) : IConsumer<CoursePublishedEvent>
{
    public async Task Consume(ConsumeContext<CoursePublishedEvent> context)
    {
        var message = context.Message;

        var existing = await unitOfWork.AggCourseStatsRepository.GetByCourseIdAsync(message.CourseId);
        if (existing == null)
        {
            var stats = new AggCourseStats
            {
                CourseId = message.CourseId,
                CourseTitle = message.CourseName,
                InstructorId = message.InstructorId,
                InstructorName = message.InstructorName
            };
            await unitOfWork.AggCourseStatsRepository.AddAsync(stats);
        }

        var instructorRevenue = await unitOfWork.AggInstructorRevenueRepository.GetByInstructorIdAsync(message.InstructorId);
        if (instructorRevenue == null)
        {
            instructorRevenue = new AggInstructorRevenue
            {
                InstructorId = message.InstructorId,
                InstructorName = message.InstructorName,
                TotalCourses = 1
            };
            await unitOfWork.AggInstructorRevenueRepository.AddAsync(instructorRevenue);
        }
        else
        {
            instructorRevenue.TotalCourses++;
            if (string.IsNullOrEmpty(instructorRevenue.InstructorName))
                instructorRevenue.InstructorName = message.InstructorName;
            await unitOfWork.AggInstructorRevenueRepository.UpdateAsync(instructorRevenue.Id, instructorRevenue);
        }

        var overview = await unitOfWork.AggSystemOverviewRepository.GetOrCreateCurrentAsync();
        overview.TotalCourses++;
        overview.TotalPublishedCourses++;

        await unitOfWork.SaveChangesAsync();

        logger.LogInformation("Course published analytics updated: {CourseId} by {InstructorId}",
            message.CourseId, message.InstructorId);
    }
}
