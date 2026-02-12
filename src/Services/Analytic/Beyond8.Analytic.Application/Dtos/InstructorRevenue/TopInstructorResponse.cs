namespace Beyond8.Analytic.Application.Dtos.InstructorRevenue;

public class TopInstructorResponse
{
    public Guid InstructorId { get; set; }
    public string InstructorName { get; set; } = string.Empty;
    public int TotalStudents { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AvgCourseRating { get; set; }
    public int TotalCourses { get; set; }
}
