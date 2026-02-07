namespace Beyond8.Sale.Application.Dtos.Courses;

/// <summary>
/// Minimal course DTO for Order Service - maps from Catalog's CourseSummaryResponse
/// </summary>
public class CourseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal FinalPrice { get; set; }
    public Guid InstructorId { get; set; }
    public string InstructorName { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
}
