using Beyond8.Catalog.Domain.Enums;

namespace Beyond8.Catalog.Application.Dtos.Lessons;

public class LessonSimpleResponse
{
    public Guid Id { get; set; }
    public Guid SectionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public LessonType Type { get; set; }
    public int OrderIndex { get; set; }
    public bool IsPreview { get; set; }
    public bool IsPublished { get; set; }
}
