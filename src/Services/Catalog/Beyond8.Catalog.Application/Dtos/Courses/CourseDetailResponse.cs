using Beyond8.Catalog.Domain.Enums;

namespace Beyond8.Catalog.Application.Dtos.Courses;

public class CourseDetailResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public Guid InstructorId { get; set; }
    public string InstructorName { get; set; } = string.Empty;
    public CourseStatus Status { get; set; }
    public CourseLevel Level { get; set; }
    public string Language { get; set; } = "vi-VN";
    public decimal Price { get; set; }
    public string ThumbnailUrl { get; set; } = string.Empty;

    // Learning content
    public List<string> Outcomes { get; set; } = [];
    public List<string>? Requirements { get; set; }
    public List<string>? TargetAudience { get; set; }

    // Statistics
    public int TotalStudents { get; set; }
    public int TotalSections { get; set; }
    public int TotalLessons { get; set; }
    public int TotalDurationMinutes { get; set; }
    public decimal? AvgRating { get; set; }
    public int TotalReviews { get; set; }

    // Content structure
    public List<SectionSummaryDto> Sections { get; set; } = [];

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
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