using Beyond8.Catalog.Domain.Repositories.Interfaces;
using Beyond8.Common.Events.Assessment;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Beyond8.Catalog.Application.Consumers.Assessment
{
    public class QuizDeletedEventConsumer(ILogger<QuizDeletedEventConsumer> logger, IUnitOfWork unitOfWork) : IConsumer<QuizDeletedEvent>
    {
        public async Task Consume(ConsumeContext<QuizDeletedEvent> context)
        {
            try
            {
                var message = context.Message;
                logger.LogInformation("Consuming quiz deleted event: {QuizId}", message.QuizId);

                var lessons = await unitOfWork.LessonRepository.AsQueryable()
                    .Include(l => l.Quiz)
                    .Where(l => l.Quiz != null && l.Quiz.QuizId == message.QuizId)
                    .ToListAsync();

                if (lessons.Count == 0)
                {
                    logger.LogInformation("No lessons found for quiz: {QuizId}", message.QuizId);
                    return;
                }

                foreach (var lesson in lessons)
                {
                    lesson.Quiz!.QuizId = null;
                    await unitOfWork.LessonRepository.UpdateAsync(lesson.Id, lesson);
                }

                await unitOfWork.SaveChangesAsync();

                logger.LogInformation("Quiz deleted event: {QuizId} processed successfully", message.QuizId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error consuming quiz deleted event: {QuizId}", context.Message.QuizId);
            }
        }
    }
}