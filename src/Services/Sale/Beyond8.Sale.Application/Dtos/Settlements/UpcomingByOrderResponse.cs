using Beyond8.Sale.Domain.Enums;

namespace Beyond8.Sale.Application.Dtos.Settlements;

public class UpcomingByOrderResponse
{
    public Guid OrderId { get; set; }

    // Instructor share that will be released
    public decimal InstructorAmount { get; set; }
    // Platform share that will be released
    public decimal PlatformAmount { get; set; }
    public DateTime? AvailableAt { get; set; }
    public string Currency { get; set; } = "VND";

    // Status indicators to show whether instructor/platform amounts are still pending or already completed
    public TransactionStatus? InstructorStatus { get; set; }
    public TransactionStatus? PlatformStatus { get; set; }
}
