using System;

namespace Beyond8.Catalog.Application.Dtos.LessonDocuments;

public class LessonDocumentResponse
{
    public Guid Id { get; set; }
    public Guid LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string LessonDocumentUrl { get; set; } = string.Empty;
    public bool IsDownloadable { get; set; }
    public int DownloadCount { get; set; }
    public bool IsIndexedInVectorDb { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}