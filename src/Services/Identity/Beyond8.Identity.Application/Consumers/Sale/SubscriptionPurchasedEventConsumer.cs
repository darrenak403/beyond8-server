using Beyond8.Common.Events.Sale;
using Beyond8.Identity.Domain.Entities;
using Beyond8.Identity.Domain.Enums;
using Beyond8.Identity.Domain.Repositories.Interfaces;
using MassTransit;
using Microsoft.EntityFrameworkCore;
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
                Status = SubscriptionStatus.Active,
                TotalRemainingRequests = plan.TotalRequestsInPeriod,
                RemainingRequestsPerWeek = plan.MaxRequestsPerWeek,
                RequestLimitedEndsAt = null,
                OrderId = message.OrderId,
                CreatedAt = now,
                CreatedBy = message.UserId
            };

            // Expire existing active subscriptions for this user so the new plan overrides them
            try
            {
                var existingActive = await unitOfWork.UserSubscriptionRepository.AsQueryable()
                    .Where(us => us.UserId == message.UserId && us.Status == SubscriptionStatus.Active)
                    .ToListAsync();

                foreach (var ex in existingActive)
                {
                    ex.Status = SubscriptionStatus.Expired;
                    // Set ExpiresAt to new subscription start so it is effectively overridden
                    ex.ExpiresAt = userSubscription.StartedAt;
                    ex.UpdatedAt = now;
                }

                if (existingActive.Any())
                {
                    await unitOfWork.SaveChangesAsync();
                    logger.LogInformation("Expired {Count} existing subscriptions for user {UserId} due to new purchase", existingActive.Count, message.UserId);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to expire existing subscriptions for user {UserId}", message.UserId);
            }

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
