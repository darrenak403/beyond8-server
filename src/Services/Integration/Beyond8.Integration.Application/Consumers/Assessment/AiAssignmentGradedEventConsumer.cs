using Beyond8.Common.Events.Assessment;
using Beyond8.Integration.Application.Dtos.Notifications;
using Beyond8.Integration.Application.Mappings.NotificationMappings;
using Beyond8.Integration.Application.Services.Interfaces;
using Beyond8.Integration.Domain.Enums;
using Beyond8.Integration.Domain.Repositories.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Integration.Application.Consumers.Assessment
{
    public class AiAssignmentGradedEventConsumer(
        ILogger<AiAssignmentGradedEventConsumer> logger,
        IUnitOfWork unitOfWork,
        INotificationService notificationService) : IConsumer<AiAssignmentGradedEvent>
    {
        public async Task Consume(ConsumeContext<AiAssignmentGradedEvent> context)
        {
            var message = context.Message;
            try
            {
                logger.LogInformation("Consuming AiAssignmentGradedEvent: SubmissionId={SubmissionId}, AssignmentId={AssignmentId}, SectionId={SectionId}, StudentId={StudentId}",
                    message.SubmissionId, message.AssignmentId, message.SectionId, message.StudentId);

                var data = new DataInfor
                {
                    Title = "Bài tập đã được chấm bởi AI",
                    Message = $"Điểm số của bạn trong bài tập {message.AssignmentTitle} là {message.Score}.",
                    Metadata = new
                    {
                        submissionId = message.SubmissionId,
                    }
                };

                await notificationService.SendToUserAsync(message.StudentId.ToString(), "AiAssignmentGraded", data);

                try
                {
                    await unitOfWork.NotificationRepository.AddAsync(message.AiAssignmentGradedEventToNotification(NotificationStatus.Delivered, data));
                    await unitOfWork.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error saving notification for AiAssignmentGradedEvent: SubmissionId={SubmissionId}, AssignmentId={AssignmentId}, SectionId={SectionId}, StudentId={StudentId}",
                        message.SubmissionId, message.AssignmentId, message.SectionId, message.StudentId);
                    throw;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error consuming AiAssignmentGradedEvent: SubmissionId={SubmissionId}, AssignmentId={AssignmentId}, SectionId={SectionId}, StudentId={StudentId}",
                    message.SubmissionId, message.AssignmentId, message.SectionId, message.StudentId);
                throw;
            }
        }

    }
}