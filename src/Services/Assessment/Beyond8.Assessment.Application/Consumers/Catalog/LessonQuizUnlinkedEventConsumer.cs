using Beyond8.Assessment.Domain.Repositories.Interfaces;
using Beyond8.Common.Events.Catalog;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Assessment.Application.Consumers.Catalog;

public class LessonQuizUnlinkedEventConsumer(
    ILogger<LessonQuizUnlinkedEventConsumer> logger,
    IUnitOfWork unitOfWork) : IConsumer<LessonQuizUnlinkedEvent>
{
    public async Task Consume(ConsumeContext<LessonQuizUnlinkedEvent> context)
    {
        try
        {
            var message = context.Message;
            logger.LogInformation("Consuming lesson quiz unlinked event: LessonId={LessonId}, QuizId={QuizId}",
                message.LessonId, message.QuizId);

            var quiz = await unitOfWork.QuizRepository.FindOneAsync(q =>
                q.Id == message.QuizId && q.LessonId == message.LessonId);

            if (quiz == null)
            {
                logger.LogInformation("Quiz {QuizId} not found or not linked to lesson {LessonId}", message.QuizId, message.LessonId);
                return;
            }

            quiz.LessonId = null;
            await unitOfWork.QuizRepository.UpdateAsync(quiz.Id, quiz);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Quiz {QuizId} LessonId cleared after unlink from lesson {LessonId}", message.QuizId, message.LessonId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error consuming lesson quiz unlinked event: QuizId={QuizId}", context.Message.QuizId);
        }
    }
}
