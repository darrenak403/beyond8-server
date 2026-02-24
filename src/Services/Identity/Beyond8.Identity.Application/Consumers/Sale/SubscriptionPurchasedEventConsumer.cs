using Beyond8.Common.Events.Sale;
using Beyond8.Identity.Domain.Entities;
using Beyond8.Identity.Domain.Repositories.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Identity.Application.Consumers.Sale;

public class SubscriptionPurchasedEventConsumer(
    ILogger<SubscriptionPurchasedEventConsumer> logger,
    IUnitOfWork unitOfWork) : IConsumer<SubscriptionPurchasedEvent>
{
    public async Task Consume(ConsumeContext<SubscriptionPurchasedEvent> context)
    {
        var message = context.Message;

        try
        {
            var user = await unitOfWork.UserRepository.FindOneAsync(u => u.Id == message.UserId);
            if (user == null)
            {
                logger.LogWarning("User {UserId} not found when processing SubscriptionPurchasedEvent", message.UserId);
                return;
            }

            var plan = await unitOfWork.SubscriptionPlanRepository.FindOneAsync(p => p.Id == message.PlanId);
            if (plan == null)
            {
                logger.LogWarning("Subscription plan {PlanId} not found when processing SubscriptionPurchasedEvent", message.PlanId);
                return;
            }

            var now = DateTime.UtcNow;

            var userSubscription = new UserSubscription
            {
                UserId = message.UserId,
                PlanId = message.PlanId,
                StartedAt = message.StartedAt == default ? now : message.StartedAt,
                ExpiresAt = message.ExpiresAt,
                Status = Domain.Enums.SubscriptionStatus.Active,
                TotalRemainingRequests = plan.TotalRequestsInPeriod,
                RemainingRequestsPerWeek = plan.MaxRequestsPerWeek,
                RequestLimitedEndsAt = null,
                OrderId = message.OrderId,
                CreatedAt = now,
                CreatedBy = message.UserId
            };

            await unitOfWork.UserSubscriptionRepository.AddAsync(userSubscription);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Created subscription for user {UserId} plan {PlanId} (Order={OrderId})", message.UserId, message.PlanId, message.OrderId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing SubscriptionPurchasedEvent: OrderId={OrderId}, UserId={UserId}", message.OrderId, message.UserId);
            throw;
        }
    }
}
