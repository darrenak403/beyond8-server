namespace Beyond8.Analytic.Application.Dtos.Stats;

public class InstructorCourseStatsResponse
{
    public int TotalCourses { get; set; }
    public int DraftCourses { get; set; }
    public int PendingApprovalCourses { get; set; }
    public int PublishedCourses { get; set; }
    public int RejectedCourses { get; set; }
    public int TotalStudents { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    // Month-over-month for published courses (field names match Catalog's CourseStatsResponse)
    public int CoursesThisMonth { get; set; }
    public int CoursesLastMonth { get; set; }
}
