using Beyond8.Common.Events.Catalog;
using Beyond8.Learning.Application.Helpers;
using Beyond8.Learning.Domain.Enums;
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
            var lessonProgresses = (await unitOfWork.LessonProgressRepository.GetAllAsync(lp =>
                lp.LessonId == msg.LessonId)).ToList();

            if (lessonProgresses.Count == 0)
            {
                logger.LogDebug("No LessonProgress found for LessonId {LessonId}, skipping duration sync", msg.LessonId);
                return;
            }

            foreach (var lp in lessonProgresses)
            {
                lp.TotalDurationSeconds = msg.DurationSeconds;

                lp.WatchPercent = lp.TotalDurationSeconds > 0
                    ? Math.Min(100, (decimal)lp.LastPositionSeconds * 100 / lp.TotalDurationSeconds)
                    : 0;

                if (lp.WatchPercent < 100 && lp.Status == LessonProgressStatus.Completed && !lp.IsManuallyCompleted)
                {
                    lp.Status = LessonProgressStatus.InProgress;
                    lp.CompletedAt = null;
                }

                await unitOfWork.LessonProgressRepository.UpdateAsync(lp.Id, lp);
            }

            var enrollmentIds = lessonProgresses.Select(lp => lp.EnrollmentId).Distinct().ToList();
            foreach (var enrollmentId in enrollmentIds)
            {
                var enrollment = await unitOfWork.EnrollmentRepository.FindOneAsync(e =>
                    e.Id == enrollmentId && e.DeletedAt == null);
                if (enrollment == null) continue;

                var completedCount = (int)await unitOfWork.LessonProgressRepository.CountAsync(l =>
                    l.EnrollmentId == enrollmentId &&
                    (l.Status == LessonProgressStatus.Completed || l.Status == LessonProgressStatus.Failed));
                EnrollmentProgressHelper.ApplyProgressToEnrollment(enrollment, completedCount, DateTime.UtcNow);
                await unitOfWork.EnrollmentRepository.UpdateAsync(enrollment.Id, enrollment);
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
