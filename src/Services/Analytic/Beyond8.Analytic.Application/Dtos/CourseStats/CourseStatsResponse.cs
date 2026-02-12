namespace Beyond8.Analytic.Application.Dtos.CourseStats;

public class CourseStatsResponse
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public Guid InstructorId { get; set; }
    public string InstructorName { get; set; } = string.Empty;
    public int TotalStudents { get; set; }
    public int TotalCompletedStudents { get; set; }
    public decimal CompletionRate { get; set; }
    public decimal? AvgRating { get; set; }
    public int TotalReviews { get; set; }
    public int TotalRatings { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalRefundAmount { get; set; }
    public decimal NetRevenue { get; set; }
    public int TotalViews { get; set; }
    public decimal AvgWatchTime { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
