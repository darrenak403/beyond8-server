using Beyond8.Catalog.Application.Dtos.LessonDocuments;
using Beyond8.Common.Utilities;

namespace Beyond8.Catalog.Application.Services.Interfaces;

public interface ILessonDocumentService
{
    Task<ApiResponse<List<LessonDocumentResponse>>> GetLessonDocumentsAsync(Guid lessonId, Guid currentUserId);
    Task<ApiResponse<LessonDocumentResponse>> GetLessonDocumentByIdAsync(Guid documentId, Guid currentUserId);
    Task<ApiResponse<LessonDocumentResponse>> CreateLessonDocumentAsync(CreateLessonDocumentRequest request, Guid currentUserId);
    Task<ApiResponse<LessonDocumentResponse>> UpdateLessonDocumentAsync(Guid documentId, UpdateLessonDocumentRequest request, Guid currentUserId);
    Task<ApiResponse<bool>> DeleteLessonDocumentAsync(Guid documentId, Guid currentUserId);
    Task<ApiResponse<bool>> ToggleDownloadableAsync(Guid documentId, Guid currentUserId);
    Task<ApiResponse<bool>> IncrementDownloadCountAsync(Guid documentId);
    Task<ApiResponse<bool>> UpdateVectorIndexStatusAsync(Guid documentId, bool isIndexed, Guid currentUserId);
    Task<ApiResponse<List<LessonDocumentResponse>>> GetLessonDocumentsPreviewAsync(Guid lessonId);
}
