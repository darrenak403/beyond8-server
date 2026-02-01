using System;

namespace Beyond8.Catalog.Application.Dtos.CourseDocuments;

public class UpdateCourseDocumentRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CourseDocumentUrl { get; set; } = string.Empty;
    public bool IsDownloadable { get; set; }
}