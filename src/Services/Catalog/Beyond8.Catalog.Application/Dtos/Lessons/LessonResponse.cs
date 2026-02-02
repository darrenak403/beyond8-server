using Beyond8.Catalog.Application.Dtos.LessonDocuments;
using Beyond8.Catalog.Domain.Enums;

namespace Beyond8.Catalog.Application.Dtos.Lessons;

public class LessonResponse
{
    public Guid Id { get; set; }
    public Guid SectionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public LessonType Type { get; set; }
    public int OrderIndex { get; set; }
    public bool IsPreview { get; set; }
    public bool IsPublished { get; set; }

    // Video fields
    public string? HlsVariants { get; set; }
    public string? VideoOriginalUrl { get; set; }
    public string? VideoThumbnailUrl { get; set; }
    public int? DurationSeconds { get; set; }
    public string? VideoQualities { get; set; }
    public bool IsDownloadable { get; set; }

    // Text fields
    public string? TextContent { get; set; }

    // Quiz fields
    public Guid? QuizId { get; set; }

    // Documents
    public List<LessonDocumentResponse> Documents { get; set; } = [];

    // Statistics
    public int TotalViews { get; set; }
    public int TotalCompletions { get; set; }

    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}