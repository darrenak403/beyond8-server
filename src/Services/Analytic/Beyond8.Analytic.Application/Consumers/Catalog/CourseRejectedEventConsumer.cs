using Beyond8.Analytic.Domain.Entities;
using Beyond8.Analytic.Domain.Repositories.Interfaces;
using Beyond8.Common.Events.Catalog;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Analytic.Application.Consumers.Catalog;

public class CourseRejectedEventConsumer(
    ILogger<CourseRejectedEventConsumer> logger,
    IUnitOfWork unitOfWork) : IConsumer<CourseRejectedEvent>
{
    public async Task Consume(ConsumeContext<CourseRejectedEvent> context)
    {
        var message = context.Message;

        var instructorRevenue = await unitOfWork.AggInstructorRevenueRepository.GetByInstructorIdAsync(message.InstructorId);
        // Transition: PendingApproval → Rejected
        if (instructorRevenue == null)
        {
            // Fallback: create initial record if not yet tracked
            instructorRevenue = new AggInstructorRevenue
            {
                InstructorId = message.InstructorId,
                InstructorName = message.InstructorName,
                RejectedCourses = 1
            };
            await unitOfWork.AggInstructorRevenueRepository.AddAsync(instructorRevenue);
        }
        else
        {
            if (instructorRevenue.PendingApprovalCourses > 0)
                instructorRevenue.PendingApprovalCourses--;
            instructorRevenue.RejectedCourses++;
            await unitOfWork.AggInstructorRevenueRepository.UpdateAsync(instructorRevenue.Id, instructorRevenue);
        }

        await unitOfWork.SaveChangesAsync();

        logger.LogInformation("Course rejected analytics recorded: Course {CourseId}, Instructor {InstructorId}, RejectedCourses={Count}",
            message.CourseId, message.InstructorId, instructorRevenue.RejectedCourses);
    }
}
