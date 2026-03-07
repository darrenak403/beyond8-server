namespace Beyond8.Analytic.Application.Dtos.InstructorRevenue;

/// <summary>
/// Dữ liệu thống kê doanh thu dành cho giảng viên xem thông tin của chính mình.
/// Không bao gồm các trường nội bộ như TotalRevenue (gross), TotalPlatformFee.
/// </summary>
public class MyRevenueResponse
{
    public Guid InstructorId { get; set; }
    public string InstructorName { get; set; } = string.Empty;

    public CourseStatsGroup Courses { get; set; } = new();
    public StudentStatsGroup Students { get; set; } = new();
    public RevenueStatsGroup Revenue { get; set; } = new();
    public RatingGroup Rating { get; set; } = new();

    public DateOnly SnapshotDate { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CourseStatsGroup
{
    public int Total { get; set; }
    public int Draft { get; set; }
    public int PendingApproval { get; set; }
    public int Approved { get; set; }
    public int Published { get; set; }
    public int Rejected { get; set; }
    public int Archived { get; set; }
    public int Suspended { get; set; }
    // Month-over-month for published courses
    public int PublishedThisMonth { get; set; }
    public int PublishedLastMonth { get; set; }
    public decimal PublishedGrowthPercent { get; set; }
    public int PublishedGrowthAbsolute { get; set; }
}

public class StudentStatsGroup
{
    public int Total { get; set; }
    public int ThisMonth { get; set; }
    public int LastMonth { get; set; }
    public decimal GrowthPercent { get; set; }
    public int GrowthAbsolute { get; set; }
}

/// <summary>Instructor net earnings (70% of gross) and wallet balance.</summary>
public class RevenueStatsGroup
{
    public decimal TotalEarnings { get; set; }
    public decimal AvailableBalance { get; set; }
    public decimal ThisMonth { get; set; }
    public decimal LastMonth { get; set; }
    public decimal GrowthPercent { get; set; }
    public decimal GrowthAbsolute { get; set; }
}

public class RatingGroup
{
    public decimal Average { get; set; }
    public int TotalReviews { get; set; }
}
