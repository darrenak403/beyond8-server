using System;
using Beyond8.Catalog.Domain.Enums;
using Beyond8.Catalog.Domain.Repositories.Interfaces;
using Beyond8.Common.Events.Identity;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Catalog.Application.Consumers.Identity;

public class InstructorHiddenEventConsumer(ILogger<InstructorHiddenEventConsumer> logger, IUnitOfWork unitOfWork) : IConsumer<InstructorHiddenEvent>
{
    public async Task Consume(ConsumeContext<InstructorHiddenEvent> context)
    {
        var message = context.Message;

        logger.LogInformation("Consuming instructor hidden event for user {UserId}", message.UserId);

        var courses = await unitOfWork.CourseRepository.GetAllAsync(c => c.InstructorId == message.UserId);
        if (courses == null)
        {
            logger.LogWarning("Courses not found for user {UserId}", message.UserId);
            return;
        }
        
        foreach (var course in courses)
        {
            course.InstructorVerificationStatus = InstructorVerificationStatus.Hidden;
            await unitOfWork.CourseRepository.UpdateAsync(course.Id, course);
        }

        await unitOfWork.SaveChangesAsync();

        logger.LogInformation("Instructor hidden event for user {UserId} processed successfully", message.UserId);
    }
}
