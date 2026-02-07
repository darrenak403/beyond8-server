namespace Beyond8.Analytic.Application.Dtos.SystemOverview;

public class SystemOverviewResponse
{
    public int TotalCourses { get; set; }
    public int TotalPublishedCourses { get; set; }
    public int TotalEnrollments { get; set; }
    public int TotalCompletedEnrollments { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalPlatformFee { get; set; }
    public decimal TotalInstructorEarnings { get; set; }
    public decimal TotalRefundAmount { get; set; }
    public decimal AvgCourseCompletionRate { get; set; }
    public decimal AvgCourseRating { get; set; }
    public int TotalReviews { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
