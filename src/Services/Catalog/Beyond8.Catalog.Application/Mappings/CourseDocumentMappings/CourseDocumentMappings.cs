using Beyond8.Catalog.Application.Dtos.CourseDocuments;
using Beyond8.Catalog.Domain.Entities;

namespace Beyond8.Catalog.Application.Mappings.CourseDocumentMappings;

public static class CourseDocumentMappings
{
    public static CourseDocumentResponse ToResponse(this CourseDocument document)
    {
        return new CourseDocumentResponse
        {
            Id = document.Id,
            CourseId = document.CourseId,
            Title = document.Title,
            Description = document.Description,
            CourseDocumentUrl = document.CourseDocumentUrl,
            IsDownloadable = document.IsDownloadable,
            DownloadCount = document.DownloadCount,
            IsIndexedInVectorDb = document.IsIndexedInVectorDb,
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt
        };
    }
}