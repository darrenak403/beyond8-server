using Beyond8.Common.Events.Assessment;
using Beyond8.Learning.Application.Helpers;
using Beyond8.Learning.Application.Services.Interfaces;
using Beyond8.Learning.Domain.Enums;
using Beyond8.Learning.Domain.Repositories.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Learning.Application.Consumers.Assessment;

public class QuizAttemptCompletedEventConsumer(
    ILogger<QuizAttemptCompletedEventConsumer> logger,
    IUnitOfWork unitOfWork,
    ICertificateService certificateService) : IConsumer<QuizAttemptCompletedEvent>
{
    public async Task Consume(ConsumeContext<QuizAttemptCompletedEvent> context)
    {
        var msg = context.Message;

        var lp = await unitOfWork.LessonProgressRepository.FindOneAsync(l =>
            l.LessonId == msg.LessonId && l.UserId == msg.StudentId);

        if (lp == null)
        {
            logger.LogWarning("LessonProgress not found for QuizAttemptCompleted: LessonId {LessonId}, StudentId {StudentId}",
                msg.LessonId, msg.StudentId);
            return;
        }

        lp.QuizAttempts = (lp.QuizAttempts ?? 0) + 1;
        lp.QuizBestScore = lp.QuizBestScore.HasValue
            ? Math.Max(lp.QuizBestScore.Value, msg.ScorePercent)
            : msg.ScorePercent;

        if (msg.IsPassed)
        {
            lp.Status = LessonProgressStatus.Completed;
            lp.CompletedAt = msg.CompletedAt;
            if (lp.StartedAt == null)
                lp.StartedAt = msg.CompletedAt;
        }
        else if (msg.MaxAttempts > 0 && msg.AttemptNumber >= msg.MaxAttempts)
        {
            lp.Status = LessonProgressStatus.Failed;
            logger.LogWarning(
                "Student {StudentId} has used all {MaxAttempts} attempts for quiz on LessonId {LessonId} and failed all",
                msg.StudentId, msg.MaxAttempts, msg.LessonId);
        }

        await unitOfWork.LessonProgressRepository.UpdateAsync(lp.Id, lp);

        var enrollment = await unitOfWork.EnrollmentRepository.FindOneAsync(e =>
            e.Id == lp.EnrollmentId && e.DeletedAt == null);
        if (enrollment != null)
        {
            var completedCount = (int)await unitOfWork.LessonProgressRepository.CountAsync(l =>
                l.EnrollmentId == lp.EnrollmentId &&
                (l.Status == LessonProgressStatus.Completed || l.Status == LessonProgressStatus.Failed));
            EnrollmentProgressHelper.ApplyProgressToEnrollment(enrollment, completedCount, msg.CompletedAt);
            await unitOfWork.EnrollmentRepository.UpdateAsync(enrollment.Id, enrollment);
        }

        await unitOfWork.SaveChangesAsync();

        await certificateService.TryIssueCertificateIfEligibleAsync(lp.EnrollmentId);

        logger.LogInformation(
            "QuizAttemptCompleted applied: LessonId {LessonId}, StudentId {StudentId}, IsPassed {IsPassed}, ScorePercent {ScorePercent}",
            msg.LessonId, msg.StudentId, msg.IsPassed, msg.ScorePercent);
    }
}
