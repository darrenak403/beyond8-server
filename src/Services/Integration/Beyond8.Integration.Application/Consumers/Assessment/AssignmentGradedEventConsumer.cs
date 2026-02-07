using Beyond8.Common.Events.Assessment;
using Beyond8.Integration.Application.Dtos.Notifications;
using Beyond8.Integration.Application.Mappings.NotificationMappings;
using Beyond8.Integration.Application.Services.Interfaces;
using Beyond8.Integration.Domain.Enums;
using Beyond8.Integration.Domain.Repositories.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Integration.Application.Consumers.Assessment;
public class AssignmentGradedEventConsumer(
    ILogger<AssignmentGradedEventConsumer> logger,
    IUnitOfWork unitOfWork,
    INotificationService notificationService) : IConsumer<AssignmentGradedEvent>
{
    public async Task Consume(ConsumeContext<AssignmentGradedEvent> context)
    {
        var message = context.Message;
        try
        {
            logger.LogInformation("Consuming AssignmentGradedEvent: SubmissionId={SubmissionId}, AssignmentId={AssignmentId}, SectionId={SectionId}, StudentId={StudentId}",
                message.SubmissionId, message.AssignmentId, message.SectionId, message.StudentId);

            var data = new DataInfor
            {
                Title = "Bài tập đã được chấm bởi giảng viên",
                Message = $"Điểm số của bạn trong bài tập {message.AssignmentTitle} là {message.Score}.",
                Metadata = new
                {
                    submissionId = message.SubmissionId,
                }
            };

            await notificationService.SendToUserAsync(message.StudentId.ToString(), "AssignmentGraded", data);

            try
            {
                await unitOfWork.NotificationRepository.AddAsync(message.AssignmentGradedEventToNotification(NotificationStatus.Delivered, data));
                await unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error saving notification for InstructorAssignmentGradedEvent: SubmissionId={SubmissionId}, AssignmentId={AssignmentId}, SectionId={SectionId}, StudentId={StudentId}",
                    message.SubmissionId, message.AssignmentId, message.SectionId, message.StudentId);
                throw;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error consuming InstructorAssignmentGradedEvent: SubmissionId={SubmissionId}, AssignmentId={AssignmentId}, SectionId={SectionId}, StudentId={StudentId}",
                message.SubmissionId, message.AssignmentId, message.SectionId, message.StudentId);
            throw;
        }
    }
}
