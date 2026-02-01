using System;
using Beyond8.Catalog.Application.Dtos.Lessons;

namespace Beyond8.Catalog.Application.Dtos.Sections;

public class SectionSummaryResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Order { get; set; }
    public int TotalLessons { get; set; }
    public int TotalDurationMinutes { get; set; }
    public List<LessonSummaryResponse> Lessons { get; set; } = [];
}
