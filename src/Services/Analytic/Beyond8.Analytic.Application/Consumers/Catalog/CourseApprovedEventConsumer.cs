using Beyond8.Analytic.Domain.Repositories.Interfaces;
using Beyond8.Common.Events.Catalog;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Analytic.Application.Consumers.Catalog;

public class CourseApprovedEventConsumer(
    ILogger<CourseApprovedEventConsumer> logger,
    IUnitOfWork unitOfWork) : IConsumer<CourseApprovedEvent>
{
    public async Task Consume(ConsumeContext<CourseApprovedEvent> context)
    {
        var message = context.Message;

        // Ensure AggInstructorRevenue record exists for this instructor
        var instructorRevenue = await unitOfWork.AggInstructorRevenueRepository.GetByInstructorIdAsync(message.InstructorId);
        if (instructorRevenue == null)
        {
            logger.LogWarning("AggInstructorRevenue not found for instructor {InstructorId} on CourseApprovedEvent",
                message.InstructorId);
            return;
        }

        await unitOfWork.SaveChangesAsync();

        logger.LogInformation("Course approved analytics recorded: Course {CourseId}, Instructor {InstructorId}",
            message.CourseId, message.InstructorId);
    }
}
