namespace Beyond8.Catalog.Application.Dtos.LessonDocuments;

public class CreateLessonDocumentRequest
{
    public Guid LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string LessonDocumentUrl { get; set; } = string.Empty;
    public bool IsDownloadable { get; set; } = false;
    public bool IsIndexedInVectorDb { get; set; } = false;
}
