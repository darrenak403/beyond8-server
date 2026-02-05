using Beyond8.Common.Events.Assessment;
using Beyond8.Learning.Application.Services.Interfaces;
using Beyond8.Learning.Domain.Repositories.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Learning.Application.Consumers.Assessment;

public class AiGradingCompletedEventConsumer(
    ILogger<AiGradingCompletedEventConsumer> logger,
    IUnitOfWork unitOfWork,
    ICertificateService certificateService) : IConsumer<AiGradingCompletedEvent>
{
    public async Task Consume(ConsumeContext<AiGradingCompletedEvent> context)
    {
        var msg = context.Message;
        if (!msg.SectionId.HasValue)
        {
            logger.LogDebug("AiGradingCompletedEvent has no SectionId, skipping Learning update: SubmissionId {SubmissionId}",
                msg.SubmissionId);
            return;
        }

        var sp = await unitOfWork.SectionProgressRepository.FindOneAsync(s =>
            s.SectionId == msg.SectionId.Value && s.UserId == msg.StudentId);

        if (sp == null)
        {
            logger.LogWarning("SectionProgress not found for AiGradingCompleted: SectionId {SectionId}, StudentId {StudentId}",
                msg.SectionId, msg.StudentId);
            return;
        }

        sp.AssignmentGrade = msg.AiScore;
        sp.AssignmentGradedAt = msg.GradedAt;
        await unitOfWork.SectionProgressRepository.UpdateAsync(sp.Id, sp);
        await unitOfWork.SaveChangesAsync();

        await certificateService.TryIssueCertificateIfEligibleAsync(sp.EnrollmentId);

        logger.LogInformation(
            "AiGradingCompleted applied: SectionId {SectionId}, StudentId {StudentId}, Score {Score}, IsSuccess {IsSuccess}",
            msg.SectionId, msg.StudentId, msg.AiScore, msg.IsSuccess);
    }
}
