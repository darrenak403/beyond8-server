using System.ComponentModel.DataAnnotations;

public class UpdateInstructorStatisticsRequest
{
    public int TotalStudents { get; set; }
    public int TotalCourses { get; set; }
    public decimal AvgRating { get; set; }
}