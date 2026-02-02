using Beyond8.Catalog.Application.Dtos.LessonDocuments;
using Beyond8.Catalog.Domain.Entities;

namespace Beyond8.Catalog.Application.Mappings.LessonDocumentMappings;

public static class LessonDocumentMappings
{
    public static LessonDocumentResponse ToResponse(this LessonDocument document)
    {
        return new LessonDocumentResponse
        {
            Id = document.Id,
            LessonId = document.LessonId,
            Title = document.Title,
            Description = document.Description,
            LessonDocumentUrl = document.LessonDocumentUrl,
            IsDownloadable = document.IsDownloadable,
            DownloadCount = document.DownloadCount,
            IsIndexedInVectorDb = document.IsIndexedInVectorDb,
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt ?? document.CreatedAt
        };
    }

    public static LessonDocument ToEntity(this CreateLessonDocumentRequest request)
    {
        return new LessonDocument
        {
            LessonId = request.LessonId,
            Title = request.Title,
            Description = request.Description,
            LessonDocumentUrl = request.LessonDocumentUrl,
            IsDownloadable = request.IsDownloadable,
            IsIndexedInVectorDb = request.IsIndexedInVectorDb
        };
    }

    public static void UpdateFrom(this LessonDocument document, UpdateLessonDocumentRequest request)
    {
        document.Title = request.Title;
        document.Description = request.Description;
        document.LessonDocumentUrl = request.LessonDocumentUrl;
        document.IsDownloadable = request.IsDownloadable;
        document.IsIndexedInVectorDb = request.IsIndexedInVectorDb;
        document.UpdatedAt = DateTime.UtcNow;
    }
}