using System;

namespace Beyond8.Catalog.Application.Dtos.Courses;

public class SectionSummaryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Order { get; set; }
    public int TotalLessons { get; set; }
    public int TotalDurationMinutes { get; set; }
    public List<LessonSummaryDto> Lessons { get; set; } = [];
}
