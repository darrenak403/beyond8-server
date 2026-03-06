using Beyond8.Analytic.Domain.Entities;
using Beyond8.Analytic.Domain.Repositories.Interfaces;
using Beyond8.Common.Events.Catalog;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Analytic.Application.Consumers.Catalog;

/// <summary>
/// Tracks course creation (Draft state).
/// Transition: (none) → Draft
/// </summary>
public class CourseCreatedEventConsumer(
    ILogger<CourseCreatedEventConsumer> logger,
    IUnitOfWork unitOfWork) : IConsumer<CourseCreatedEvent>
{
    public async Task Consume(ConsumeContext<CourseCreatedEvent> context)
    {
        var message = context.Message;

        var instructorRevenue = await unitOfWork.AggInstructorRevenueRepository.GetByInstructorIdAsync(message.InstructorId);
        if (instructorRevenue == null)
        {
            instructorRevenue = new AggInstructorRevenue
            {
                InstructorId = message.InstructorId,
                InstructorName = message.InstructorName,
                TotalCourses = 1,
                DraftCourses = 1
            };
            await unitOfWork.AggInstructorRevenueRepository.AddAsync(instructorRevenue);
        }
        else
        {
            instructorRevenue.TotalCourses++;
            instructorRevenue.DraftCourses++;
            if (string.IsNullOrEmpty(instructorRevenue.InstructorName))
                instructorRevenue.InstructorName = message.InstructorName;
            await unitOfWork.AggInstructorRevenueRepository.UpdateAsync(instructorRevenue.Id, instructorRevenue);
        }

        await unitOfWork.SaveChangesAsync();

        logger.LogInformation("Course created analytics recorded: Course {CourseId}, Instructor {InstructorId}, DraftCourses={Count}",
            message.CourseId, message.InstructorId, instructorRevenue.DraftCourses);
    }
}
