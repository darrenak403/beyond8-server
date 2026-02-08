namespace Beyond8.Sale.Application.Dtos.Courses;

/// <summary>
/// Minimal course DTO for Order Service - contains only essential fields
/// </summary>
public class CourseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal OriginalPrice { get; set; }
    public Guid InstructorId { get; set; }
    public string InstructorName { get; set; } = string.Empty;
    public string? Thumbnail { get; set; }
}
