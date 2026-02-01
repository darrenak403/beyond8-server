using System.ComponentModel.DataAnnotations;

namespace Beyond8.Catalog.Application.Dtos.Lessons;

public class CreateTextLessonRequest
{
    public Guid SectionId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsPreview { get; set; } = false;

    public string Content { get; set; } = string.Empty;
}