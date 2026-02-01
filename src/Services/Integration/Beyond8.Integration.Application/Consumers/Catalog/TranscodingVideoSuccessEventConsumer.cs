using Beyond8.Common.Events.Catalog;
using Beyond8.Integration.Application.Dtos.Notifications;
using Beyond8.Integration.Application.Mappings.NotificationMappings;
using Beyond8.Integration.Application.Services.Interfaces;
using Beyond8.Integration.Domain.Enums;
using Beyond8.Integration.Domain.Repositories.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Integration.Application.Consumers.Catalog;

public class TranscodingVideoSuccessEventConsumer(
    ILogger<TranscodingVideoSuccessEventConsumer> logger,
    IUnitOfWork unitOfWork,
    INotificationService notificationService) : IConsumer<TranscodingVideoSuccessEvent>
{
    public async Task Consume(ConsumeContext<TranscodingVideoSuccessEvent> context)
    {
        var message = context.Message;

        try
        {
            logger.LogInformation("Consuming transcoding video success event for lesson {LessonId}", message.LessonId);
            var data = new DataInfor
            {
                Title = "Video đã được upload",
                Message = $"Video của bài học {message.LessonTitle} đã được upload thành công.",
                Metadata = new
                {
                    lessonId = message.LessonId
                }
            };

            await notificationService.SendToUserAsync(message.InstructorId.ToString(), "TranscodingVideoSuccess", data);

            try
            {
                await unitOfWork.NotificationRepository.AddAsync(message.TranscodingVideoSuccessEventToNotification(NotificationStatus.Delivered));
                await unitOfWork.SaveChangesAsync();
                logger.LogInformation("Successfully saved notification for transcoding video success event for lesson {LessonId}", message.LessonId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error saving notification for transcoding video success event for lesson {LessonId}", message.LessonId);
                throw;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error consuming transcoding video success event for lesson {LessonId}", message.LessonId);
            throw;
        }
    }
}
