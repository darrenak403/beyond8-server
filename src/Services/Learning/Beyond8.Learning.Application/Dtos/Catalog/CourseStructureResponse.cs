namespace Beyond8.Learning.Application.Dtos.Catalog;

public class CourseStructureResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? ThumbnailUrl { get; set; }
    public Guid InstructorId { get; set; }
    public string InstructorName { get; set; } = string.Empty;
    public int TotalLessons { get; set; }
    public int Status { get; set; }
    public List<SectionStructureItem> Sections { get; set; } = [];
}
