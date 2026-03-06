namespace Beyond8.Sale.Application.Dtos.Analytics;

public class DailyRevenueSummary
{
    /// <summary>ISO date key yyyy-MM-dd</summary>
    public string DateKey { get; set; } = string.Empty;

    public int Year { get; set; }
    public int Month { get; set; }
    public int Day { get; set; }

    /// <summary>Sum of OrderItem.LineTotal for completed orders on this day</summary>
    public decimal Revenue { get; set; }

    /// <summary>Sum of OrderItem.PlatformFeeAmount</summary>
    public decimal PlatformFee { get; set; }

    /// <summary>Sum of OrderItem.InstructorEarnings</summary>
    public decimal InstructorEarnings { get; set; }

    /// <summary>Number of OrderItems (= enrollments) on this day</summary>
    public int NewEnrollments { get; set; }
}
