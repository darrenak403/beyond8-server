using Beyond8.Analytic.Domain.Entities;
using Beyond8.Analytic.Domain.Repositories.Interfaces;
using Beyond8.Common.Events.Assessment;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Analytic.Application.Consumers.Assessment;

public class QuizAttemptCompletedEventConsumer(
    ILogger<QuizAttemptCompletedEventConsumer> logger,
    IUnitOfWork unitOfWork) : IConsumer<QuizAttemptCompletedEvent>
{
    public async Task Consume(ConsumeContext<QuizAttemptCompletedEvent> context)
    {
        var message = context.Message;

        var lesson = await unitOfWork.AggLessonPerformanceRepository.GetByLessonIdAsync(message.LessonId);
        if (lesson == null)
        {
            if (!message.CourseId.HasValue || message.CourseId.Value == Guid.Empty ||
                !message.InstructorId.HasValue || message.InstructorId.Value == Guid.Empty)
            {
                logger.LogWarning("QuizAttemptCompletedEvent: Cannot create AggLessonPerformance for LessonId {LessonId}, missing CourseId or InstructorId", message.LessonId);
                return;
            }
            lesson = new AggLessonPerformance
            {
                LessonId = message.LessonId,
                LessonTitle = string.Empty,
                CourseId = message.CourseId.Value,
                InstructorId = message.InstructorId.Value
            };
            await unitOfWork.AggLessonPerformanceRepository.AddAsync(lesson);
        }

        lesson.TotalViews++;
        if (message.IsPassed)
            lesson.TotalCompletions++;

        lesson.CompletionRate = lesson.TotalViews > 0
            ? Math.Round((decimal)lesson.TotalCompletions / lesson.TotalViews * 100, 2)
            : 0;

        await unitOfWork.SaveChangesAsync();

        logger.LogInformation("Quiz attempt analytics updated: Lesson {LessonId}, Passed={IsPassed}",
            message.LessonId, message.IsPassed);
    }
}
