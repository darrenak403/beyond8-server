namespace Beyond8.Catalog.Application.Dtos.Courses;

public class CourseStatsDto
{
    public int TotalCourses { get; set; }
    public int DraftCourses { get; set; }
    public int PendingApprovalCourses { get; set; }
    public int PublishedCourses { get; set; }
    public int RejectedCourses { get; set; }
    public int TotalStudents { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }

    // Recent activity
    public int CoursesThisMonth { get; set; }
    public int StudentsThisMonth { get; set; }
    public decimal RevenueThisMonth { get; set; }
}