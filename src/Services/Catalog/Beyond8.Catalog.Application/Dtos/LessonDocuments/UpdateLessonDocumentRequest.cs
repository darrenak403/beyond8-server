namespace Beyond8.Catalog.Application.Dtos.LessonDocuments;

public class UpdateLessonDocumentRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string LessonDocumentUrl { get; set; } = string.Empty;
    public bool IsDownloadable { get; set; }
    public bool IsIndexedInVectorDb { get; set; }
}