using Beyond8.Common.Events.Assessment;
using Beyond8.Learning.Domain.Repositories.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Learning.Application.Consumers.Assessment;

public class AssignmentSubmissionsResetEventConsumer(
    ILogger<AssignmentSubmissionsResetEventConsumer> logger,
    IUnitOfWork unitOfWork) : IConsumer<AssignmentSubmissionsResetEvent>
{
    public async Task Consume(ConsumeContext<AssignmentSubmissionsResetEvent> context)
    {
        var msg = context.Message;

        var sp = await unitOfWork.SectionProgressRepository.FindOneAsync(s =>
            s.SectionId == msg.SectionId && s.UserId == msg.StudentId);

        if (sp == null)
        {
            logger.LogWarning("SectionProgress not found for AssignmentSubmissionsReset: SectionId {SectionId}, StudentId {StudentId}",
                msg.SectionId, msg.StudentId);
            return;
        }

        var enrollment = await unitOfWork.EnrollmentRepository.FindOneAsync(e =>
            e.Id == sp.EnrollmentId && e.DeletedAt == null);

        if (enrollment?.CertificateId != null)
        {
            logger.LogWarning(
                "Skipping AssignmentSubmissionsReset: Enrollment {EnrollmentId} already has certificate {CertificateId}. SectionId {SectionId}, StudentId {StudentId}",
                sp.EnrollmentId, enrollment.CertificateId, msg.SectionId, msg.StudentId);
            return;
        }

        sp.AssignmentSubmitted = false;
        sp.AssignmentGrade = null;
        sp.AssignmentSubmittedAt = null;
        sp.AssignmentGradedAt = null;

        await unitOfWork.SectionProgressRepository.UpdateAsync(sp.Id, sp);
        await unitOfWork.SaveChangesAsync();

        logger.LogInformation(
            "AssignmentSubmissionsReset applied: SectionId {SectionId}, StudentId {StudentId}, ResetBy {InstructorId}",
            msg.SectionId, msg.StudentId, msg.ResetByInstructorId);
    }
}
