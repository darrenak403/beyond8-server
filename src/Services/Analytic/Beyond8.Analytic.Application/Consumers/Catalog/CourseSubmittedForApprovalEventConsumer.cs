using Beyond8.Analytic.Domain.Repositories.Interfaces;
using Beyond8.Common.Events.Catalog;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Analytic.Application.Consumers.Catalog;

/// <summary>
/// Tracks course submission for approval (Draft → PendingApproval).
/// Transition: DraftCourses--, PendingApprovalCourses++
/// </summary>
public class CourseSubmittedForApprovalEventConsumer(
    ILogger<CourseSubmittedForApprovalEventConsumer> logger,
    IUnitOfWork unitOfWork) : IConsumer<CourseSubmittedForApprovalEvent>
{
    public async Task Consume(ConsumeContext<CourseSubmittedForApprovalEvent> context)
    {
        var message = context.Message;

        var instructorRevenue = await unitOfWork.AggInstructorRevenueRepository.GetByInstructorIdAsync(message.InstructorId);
        if (instructorRevenue == null)
        {
            logger.LogWarning("AggInstructorRevenue not found for instructor {InstructorId} on CourseSubmittedForApprovalEvent",
                message.InstructorId);
            return;
        }

        if (instructorRevenue.DraftCourses > 0)
            instructorRevenue.DraftCourses--;
        instructorRevenue.PendingApprovalCourses++;
        await unitOfWork.AggInstructorRevenueRepository.UpdateAsync(instructorRevenue.Id, instructorRevenue);

        await unitOfWork.SaveChangesAsync();

        logger.LogInformation("Course submitted analytics recorded: Course {CourseId}, Instructor {InstructorId}, PendingApproval={Count}",
            message.CourseId, message.InstructorId, instructorRevenue.PendingApprovalCourses);
    }
}
