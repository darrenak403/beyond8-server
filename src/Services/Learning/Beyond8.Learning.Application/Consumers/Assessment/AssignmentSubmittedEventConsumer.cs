using Beyond8.Common.Events.Assessment;
using Beyond8.Learning.Application.Services.Interfaces;
using Beyond8.Learning.Domain.Repositories.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Learning.Application.Consumers.Assessment;

public class AssignmentSubmittedEventConsumer(
    ILogger<AssignmentSubmittedEventConsumer> logger,
    IUnitOfWork unitOfWork,
    ICertificateService certificateService) : IConsumer<AssignmentSubmittedEvent>
{
    public async Task Consume(ConsumeContext<AssignmentSubmittedEvent> context)
    {
        var msg = context.Message;
        if (!msg.SectionId.HasValue)
        {
            logger.LogDebug("AssignmentSubmittedEvent has no SectionId, skipping Learning update: AssignmentId {AssignmentId}",
                msg.AssignmentId);
            return;
        }

        var sp = await unitOfWork.SectionProgressRepository.FindOneAsync(s =>
            s.SectionId == msg.SectionId.Value && s.UserId == msg.StudentId);

        if (sp == null)
        {
            logger.LogWarning("SectionProgress not found for AssignmentSubmitted: SectionId {SectionId}, StudentId {StudentId}",
                msg.SectionId, msg.StudentId);
            return;
        }

        sp.AssignmentSubmitted = true;
        sp.AssignmentSubmittedAt = msg.SubmittedAt;
        await unitOfWork.SectionProgressRepository.UpdateAsync(sp.Id, sp);
        await unitOfWork.SaveChangesAsync();

        await certificateService.TryIssueCertificateIfEligibleAsync(sp.EnrollmentId);

        logger.LogInformation(
            "AssignmentSubmitted applied: SectionId {SectionId}, StudentId {StudentId}, SubmissionId {SubmissionId}",
            msg.SectionId, msg.StudentId, msg.SubmissionId);
    }
}
