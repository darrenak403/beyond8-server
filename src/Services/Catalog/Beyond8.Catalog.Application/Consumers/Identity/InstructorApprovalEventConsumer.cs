using Beyond8.Catalog.Domain.Enums;
using Beyond8.Catalog.Domain.Repositories.Interfaces;
using Beyond8.Common.Events.Identity;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Catalog.Application.Consumers.Identity;

public class InstructorApprovalEventConsumer(
    ILogger<InstructorApprovalEventConsumer> logger,
    IUnitOfWork unitOfWork) : IConsumer<InstructorApprovalEvent>
{
    public async Task Consume(ConsumeContext<InstructorApprovalEvent> context)
    {
        var message = context.Message;

        logger.LogInformation("Consuming instructor approval event for user {UserId} (restore courses to Verified)", message.UserId);

        var courses = await unitOfWork.CourseRepository.GetAllAsync(c =>
            c.InstructorId == message.UserId && c.InstructorVerificationStatus == InstructorVerificationStatus.Hidden);

        if (courses == null || courses.Count == 0)
        {
            logger.LogInformation("No hidden courses found for user {UserId}", message.UserId);
            return;
        }

        foreach (var course in courses)
        {
            course.InstructorVerificationStatus = InstructorVerificationStatus.Verified;
            if (course.Status == CourseStatus.Suspended)
            {
                course.Status = CourseStatus.Approved;
            }
            await unitOfWork.CourseRepository.UpdateAsync(course.Id, course);
        }

        await unitOfWork.SaveChangesAsync();

        logger.LogInformation("Instructor approval event for user {UserId}: restored {Count} course(s) to Verified", message.UserId, courses.Count);
    }
}
