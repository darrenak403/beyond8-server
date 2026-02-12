namespace Beyond8.Analytic.Application.Dtos.CourseStats;

public class TopCourseResponse
{
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string InstructorName { get; set; } = string.Empty;
    public int TotalStudents { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal? AvgRating { get; set; }
}
