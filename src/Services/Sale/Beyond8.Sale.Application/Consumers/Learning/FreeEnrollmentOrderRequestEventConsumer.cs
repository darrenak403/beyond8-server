using Beyond8.Common.Events.Learning;
using Beyond8.Sale.Application.Dtos.OrderItems;
using Beyond8.Sale.Application.Dtos.Orders;
using Beyond8.Sale.Application.Services.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Sale.Application.Consumers.Learning;

public class FreeEnrollmentOrderRequestEventConsumer(
    ILogger<FreeEnrollmentOrderRequestEventConsumer> logger,
    IOrderService orderService) : IConsumer<FreeEnrollmentOrderRequestEvent>
{
    public async Task Consume(ConsumeContext<FreeEnrollmentOrderRequestEvent> context)
    {
        var message = context.Message;

        try
        {
            logger.LogInformation("Processing free enrollment order request for user {UserId}, course {CourseId}, enrollment {EnrollmentId}",
                message.UserId, message.CourseId, message.EnrollmentId);

            var orderRequest = new CreateOrderRequest
            {
                Items = new List<OrderItemRequest>
                {
                    new OrderItemRequest
                    {
                        CourseId = message.CourseId
                    }
                }
            };

            var result = await orderService.CreateOrderAsync(orderRequest, message.UserId);

            if (result.IsSuccess && result.Data != null)
            {
                logger.LogInformation("Successfully created free enrollment order {OrderId} for user {UserId}, course {CourseId}",
                    result.Data.Id, message.UserId, message.CourseId);
            }
            else
            {
                logger.LogError("Failed to create free enrollment order for user {UserId}, course {CourseId}: {Message}",
                    message.UserId, message.CourseId, result.Message);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing free enrollment order request for user {UserId}, course {CourseId}",
                message.UserId, message.CourseId);
            throw;
        }
    }
}