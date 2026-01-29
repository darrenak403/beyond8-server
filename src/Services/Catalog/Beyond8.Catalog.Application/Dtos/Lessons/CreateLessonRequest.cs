using Beyond8.Catalog.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Beyond8.Catalog.Application.Dtos.Lessons;

public class CreateLessonRequest
{
    [Required]
    public Guid SectionId { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public LessonType Type { get; set; } = LessonType.Video;

    public int OrderIndex { get; set; }

    public bool IsPreview { get; set; } = false;

    // Video fields
    public string? HlsVariants { get; set; }
    public string? VideoOriginalUrl { get; set; }
    public string? VideoThumbnailUrl { get; set; }
    public int? DurationSeconds { get; set; }
    public string? VideoQualities { get; set; }
    public bool IsDownloadable { get; set; } = false;

    // Text fields
    public string? TextContent { get; set; }

    // Quiz fields
    public Guid? QuizId { get; set; }
    public int MinCompletionSeconds { get; set; } = 0;
    public int RequiredScore { get; set; } = 0;
}