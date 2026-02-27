using Beyond8.Common.Events.Catalog;
using Beyond8.Sale.Application.Services.Interfaces;
using Beyond8.Sale.Domain.Repositories.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Sale.Application.Consumers.Catalog;

public class CourseUpdatedMetadataEventConsumer(
    ILogger<CourseUpdatedMetadataEventConsumer> logger,
    IUnitOfWork unitOfWork,
    ICartService cartService) : IConsumer<CourseUpdatedMetadataEvent>
{
    public async Task Consume(ConsumeContext<CourseUpdatedMetadataEvent> context)
    {
        var message = context.Message;

        try
        {
            logger.LogInformation("Updating cart items for course {CourseId} with new metadata: Title={Title}, Price={Price}, Final={FinalPrice}, Thumbnail={ThumbnailUrl}",
                message.CourseId, message.Title, message.Price, message.FinalPrice, message.ThumbnailUrl);

            var cartItems = await unitOfWork.CartRepository.GetCartItemsByCourseIdAsync(message.CourseId);

            if (cartItems.Any())
            {
                var originalPrice = decimal.TryParse(message.Price, out var price) ? price : 0;
                var finalPrice = decimal.TryParse(message.FinalPrice, out var parsedFinalPrice) ? parsedFinalPrice : originalPrice;

                if (originalPrice == 0 || finalPrice == 0)
                {
                    foreach (var cartItem in cartItems)
                    {
                        await cartService.RemoveFromCartAsync(cartItem.Cart.UserId, cartItem.CourseId);
                    }

                    logger.LogInformation("Removed {Count} cart items because course {CourseId} became free",
                        cartItems.Count, message.CourseId);
                }
                else
                {
                    foreach (var cartItem in cartItems)
                    {
                        cartItem.CourseTitle = message.Title;
                        cartItem.OriginalPrice = originalPrice;
                        cartItem.FinalPrice = finalPrice;
                        cartItem.CourseThumbnail = message.ThumbnailUrl;
                    }

                    await unitOfWork.SaveChangesAsync();

                    logger.LogInformation("Updated metadata for {Count} cart items for course {CourseId}",
                        cartItems.Count, message.CourseId);
                }
            }
            else
            {
                logger.LogInformation("No cart items found for course {CourseId}", message.CourseId);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating cart items for course {CourseId}", message.CourseId);
            throw;
        }
    }
}