namespace Beyond8.Analytic.Application.Dtos.InstructorRevenue;

/// <summary>
/// Dữ liệu thống kê doanh thu dành cho giảng viên xem thông tin của chính mình.
/// Không bao gồm các trường nội bộ như TotalRevenue (gross), TotalPlatformFee.
/// </summary>
public class MyRevenueResponse
{
    public Guid InstructorId { get; set; }
    public string InstructorName { get; set; } = string.Empty;
    public int TotalCourses { get; set; }
    public int DraftCourses { get; set; }
    public int PendingApprovalCourses { get; set; }
    public int ApprovedCourses { get; set; }
    public int PublishedCourses { get; set; }
    public int RejectedCourses { get; set; }
    public int ArchivedCourses { get; set; }
    public int SuspendedCourses { get; set; }
    public int TotalStudents { get; set; }
    /// <summary>Net earnings after platform fee (70% of gross revenue).</summary>
    public decimal TotalInstructorEarnings { get; set; }
    public decimal AvailableBalance { get; set; }
    public decimal AvgCourseRating { get; set; }
    public int TotalReviews { get; set; }
    public DateOnly SnapshotDate { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
