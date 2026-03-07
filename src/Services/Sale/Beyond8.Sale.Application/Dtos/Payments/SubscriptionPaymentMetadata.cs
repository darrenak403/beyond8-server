namespace Beyond8.Sale.Application.Dtos.Payments;

public class SubscriptionPaymentMetadata
{
    public Guid? PlanId { get; set; }
    public string? PlanCode { get; set; }
    public Guid? UserId { get; set; }
    public bool? OverrideExisting { get; set; }
}
