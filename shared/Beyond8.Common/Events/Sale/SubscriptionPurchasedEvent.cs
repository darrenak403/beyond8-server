namespace Beyond8.Common.Events.Sale;

public record SubscriptionPurchasedEvent(
    Guid OrderId,
    Guid UserId,
    Guid PlanId,
    Guid? PaymentId,
    DateTime StartedAt,
    DateTime? ExpiresAt
);
