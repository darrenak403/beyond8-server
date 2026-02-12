using Beyond8.Common.Events.Catalog;
using Beyond8.Sale.Domain.Repositories.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Sale.Application.Consumers.Catalog;

public class CourseUpdatedMetadataEventConsumer(
    ILogger<CourseUpdatedMetadataEventConsumer> logger,
    IUnitOfWork unitOfWork) : IConsumer<CourseUpdatedMetadataEvent>
{
    public async Task Consume(ConsumeContext<CourseUpdatedMetadataEvent> context)
    {
        var message = context.Message;

        try
        {
            logger.LogInformation("Updating cart items for course {CourseId} with new metadata: Title={Title}, Thumbnail={ThumbnailUrl}",
                message.CourseId, message.Title, message.ThumbnailUrl);

            // Find all cart items for this course
            var cartItems = await unitOfWork.CartRepository.GetCartItemsByCourseIdAsync(message.CourseId);

            if (cartItems.Any())
            {
                // Update metadata for all cart items
                foreach (var cartItem in cartItems)
                {
                    cartItem.CourseTitle = message.Title;
                    cartItem.CourseThumbnail = message.ThumbnailUrl;
                }

                await unitOfWork.SaveChangesAsync();

                logger.LogInformation("Updated metadata for {Count} cart items for course {CourseId}",
                    cartItems.Count, message.CourseId);
            }
            else
            {
                logger.LogInformation("No cart items found for course {CourseId}", message.CourseId);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating cart items for course {CourseId}", message.CourseId);
            throw; // Let MassTransit handle retry
        }
    }
}