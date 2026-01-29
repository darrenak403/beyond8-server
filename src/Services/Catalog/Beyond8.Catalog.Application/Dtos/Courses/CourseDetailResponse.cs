using Beyond8.Catalog.Domain.Enums;

namespace Beyond8.Catalog.Application.Dtos.Courses;

public class CourseDetailResponse : CourseResponse
{
    public List<SectionSummaryDto> Sections { get; set; } = [];
}

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

public class LessonSummaryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public LessonType Type { get; set; }
    public int Order { get; set; }
    public int? DurationMinutes { get; set; }
    public bool HasVideo { get; set; }
    public bool HasQuiz { get; set; }
    public bool HasAssignment { get; set; }
}