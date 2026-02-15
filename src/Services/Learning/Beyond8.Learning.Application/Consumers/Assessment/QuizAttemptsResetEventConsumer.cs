using Beyond8.Common.Events.Assessment;
using Beyond8.Learning.Application.Helpers;
using Beyond8.Learning.Domain.Enums;
using Beyond8.Learning.Domain.Repositories.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Learning.Application.Consumers.Assessment;

public class QuizAttemptsResetEventConsumer(
    ILogger<QuizAttemptsResetEventConsumer> logger,
    IUnitOfWork unitOfWork) : IConsumer<QuizAttemptsResetEvent>
{
    public async Task Consume(ConsumeContext<QuizAttemptsResetEvent> context)
    {
        var msg = context.Message;

        var lp = await unitOfWork.LessonProgressRepository.FindOneAsync(l =>
            l.LessonId == msg.LessonId && l.UserId == msg.StudentId);

        if (lp == null)
        {
            logger.LogWarning("LessonProgress not found for QuizAttemptsReset: LessonId {LessonId}, StudentId {StudentId}",
                msg.LessonId, msg.StudentId);
            return;
        }

        var enrollment = await unitOfWork.EnrollmentRepository.FindOneAsync(e =>
            e.Id == lp.EnrollmentId && e.DeletedAt == null);

        if (enrollment?.CertificateId != null)
        {
            logger.LogWarning(
                "Skipping QuizAttemptsReset: Enrollment {EnrollmentId} already has certificate {CertificateId}. LessonId {LessonId}, StudentId {StudentId}",
                lp.EnrollmentId, enrollment.CertificateId, msg.LessonId, msg.StudentId);
            return;
        }

        lp.Status = LessonProgressStatus.InProgress;
        lp.QuizAttempts = 0;
        lp.QuizBestScore = null;
        lp.CompletedAt = null;

        await unitOfWork.LessonProgressRepository.UpdateAsync(lp.Id, lp);

        if (enrollment != null)
        {
            var completedCount = (int)await unitOfWork.LessonProgressRepository.CountAsync(l =>
                l.EnrollmentId == lp.EnrollmentId &&
                EnrollmentProgressHelper.IsCompletedOrFailed(l.Status));
            EnrollmentProgressHelper.ApplyProgressToEnrollment(enrollment, completedCount);
            await unitOfWork.EnrollmentRepository.UpdateAsync(enrollment.Id, enrollment);
        }

        await unitOfWork.SaveChangesAsync();

        logger.LogInformation(
            "QuizAttemptsReset applied: LessonId {LessonId}, StudentId {StudentId}, ResetBy {InstructorId}",
            msg.LessonId, msg.StudentId, msg.ResetByInstructorId);
    }
}
