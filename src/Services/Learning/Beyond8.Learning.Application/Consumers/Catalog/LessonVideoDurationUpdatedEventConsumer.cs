using Beyond8.Common.Events.Catalog;
using Beyond8.Learning.Domain.Repositories.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Learning.Application.Consumers.Catalog;

public class LessonVideoDurationUpdatedEventConsumer(
    ILogger<LessonVideoDurationUpdatedEventConsumer> logger,
    IUnitOfWork unitOfWork) : IConsumer<LessonVideoDurationUpdatedEvent>
{
    public async Task Consume(ConsumeContext<LessonVideoDurationUpdatedEvent> context)
    {
        var msg = context.Message;

        try
        {
            var lessonProgresses = await unitOfWork.LessonProgressRepository.GetAllAsync(lp =>
                lp.LessonId == msg.LessonId);

            if (lessonProgresses.Count == 0)
            {
                logger.LogDebug("No LessonProgress found for LessonId {LessonId}, skipping duration sync", msg.LessonId);
                return;
            }

            foreach (var lp in lessonProgresses)
            {
                lp.TotalDurationSeconds = msg.DurationSeconds;

                // Recalculate WatchPercent with new duration
                lp.WatchPercent = lp.TotalDurationSeconds > 0
                    ? Math.Min(100, (decimal)lp.LastPositionSeconds * 100 / lp.TotalDurationSeconds)
                    : 0;

                await unitOfWork.LessonProgressRepository.UpdateAsync(lp.Id, lp);
            }

            await unitOfWork.SaveChangesAsync();

            logger.LogInformation(
                "LessonVideoDurationUpdated applied: LessonId {LessonId}, NewDuration {Duration}s, UpdatedCount {Count}",
                msg.LessonId, msg.DurationSeconds, lessonProgresses.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error consuming LessonVideoDurationUpdatedEvent: LessonId {LessonId}", msg.LessonId);
        }
    }
}
