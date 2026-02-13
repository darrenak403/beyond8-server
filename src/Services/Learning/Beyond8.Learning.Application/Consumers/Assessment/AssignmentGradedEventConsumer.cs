using Beyond8.Common.Events.Assessment;
using Beyond8.Learning.Application.Services.Interfaces;
using Beyond8.Learning.Domain.Repositories.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Learning.Application.Consumers.Assessment;

public class AssignmentGradedEventConsumer(
    ILogger<AssignmentGradedEventConsumer> logger,
    IUnitOfWork unitOfWork,
    ICertificateService certificateService) : IConsumer<AssignmentGradedEvent>
{
    public async Task Consume(ConsumeContext<AssignmentGradedEvent> context)
    {
        var msg = context.Message;
        if (!msg.SectionId.HasValue)
        {
            logger.LogDebug("AssignmentGradedEvent has no SectionId, skipping Learning update: SubmissionId {SubmissionId}",
                msg.SubmissionId);
            return;
        }

        var sp = await unitOfWork.SectionProgressRepository.FindOneAsync(s =>
            s.SectionId == msg.SectionId.Value && s.UserId == msg.StudentId);

        if (sp == null)
        {
            logger.LogWarning("SectionProgress not found for AssignmentGraded: SectionId {SectionId}, StudentId {StudentId}",
                msg.SectionId, msg.StudentId);
            return;
        }

        sp.AssignmentGrade = msg.Score;
        sp.AssignmentGradedAt = msg.GradedAt;
        sp.AssignmentPassed = msg.ScorePercent >= msg.PassScorePercent;
        await unitOfWork.SectionProgressRepository.UpdateAsync(sp.Id, sp);
        await unitOfWork.SaveChangesAsync();

        await certificateService.TryIssueCertificateIfEligibleAsync(sp.EnrollmentId);

        logger.LogInformation(
            "AssignmentGraded (instructor override) applied: SectionId {SectionId}, StudentId {StudentId}, Score {Score}, GradedBy {GradedBy}",
            msg.SectionId, msg.StudentId, msg.Score, msg.GradedBy);
    }
}
