namespace Beyond8.Analytic.Application.Dtos.SystemOverview;

public class BackfillRevenueResponse
{
    /// <summary>Number of daily buckets written</summary>
    public int DailyRecordsUpserted { get; set; }

    /// <summary>Number of monthly buckets written</summary>
    public int MonthlyRecordsUpserted { get; set; }

    /// <summary>Total revenue amount backfilled</summary>
    public decimal TotalRevenue { get; set; }

    /// <summary>Total platform fee backfilled</summary>
    public decimal TotalPlatformFee { get; set; }

    /// <summary>Total instructor earnings backfilled</summary>
    public decimal TotalInstructorEarnings { get; set; }

    /// <summary>Total enrollments backfilled</summary>
    public int TotalEnrollments { get; set; }
}
