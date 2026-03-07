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

        // Transition: PendingApproval → Approved
        var instructorRevenue = await unitOfWork.AggInstructorRevenueRepository.GetByInstructorIdAsync(message.InstructorId);
        if (instructorRevenue == null)
        {
            logger.LogWarning("AggInstructorRevenue not found for instructor {InstructorId} on CourseApprovedEvent",
                message.InstructorId);
            return;
        }

        if (instructorRevenue.PendingApprovalCourses > 0)
            instructorRevenue.PendingApprovalCourses--;
        instructorRevenue.ApprovedCourses++;
        await unitOfWork.AggInstructorRevenueRepository.UpdateAsync(instructorRevenue.Id, instructorRevenue);

        await unitOfWork.SaveChangesAsync();

        logger.LogInformation("Course approved analytics recorded: Course {CourseId}, Instructor {InstructorId}, ApprovedCourses={Count}",
            message.CourseId, message.InstructorId, instructorRevenue.ApprovedCourses);
    }
}
