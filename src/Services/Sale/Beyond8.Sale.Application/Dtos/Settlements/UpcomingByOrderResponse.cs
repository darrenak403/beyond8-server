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
}
