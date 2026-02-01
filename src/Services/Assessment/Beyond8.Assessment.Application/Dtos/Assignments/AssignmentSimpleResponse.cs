namespace Beyond8.Assessment.Application.Dtos.Assignments;

public class AssignmentSimpleResponse
{
    public Guid Id { get; set; }
    public Guid InstructorId { get; set; }
    public Guid? CourseId { get; set; }
    public Guid? SectionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int TotalSubmissions { get; set; }
    public decimal? AverageScore { get; set; }
    public DateTime CreatedAt { get; set; }
}
