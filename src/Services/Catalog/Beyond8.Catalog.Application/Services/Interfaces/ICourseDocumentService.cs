using Beyond8.Catalog.Application.Dtos.CourseDocuments;
using Beyond8.Common.Utilities;

namespace Beyond8.Catalog.Application.Services.Interfaces;

public interface ICourseDocumentService
{
    Task<ApiResponse<List<CourseDocumentResponse>>> GetCourseDocumentsAsync(Guid courseId, Guid currentUserId);
    Task<ApiResponse<CourseDocumentResponse>> GetCourseDocumentByIdAsync(Guid documentId, Guid currentUserId);
    Task<ApiResponse<CourseDocumentResponse>> CreateCourseDocumentAsync(CreateCourseDocumentRequest request, Guid currentUserId);
    Task<ApiResponse<CourseDocumentResponse>> UpdateCourseDocumentAsync(Guid documentId, UpdateCourseDocumentRequest request, Guid currentUserId);
    Task<ApiResponse<bool>> DeleteCourseDocumentAsync(Guid documentId, Guid currentUserId);
    Task<ApiResponse<bool>> ToggleDownloadableAsync(Guid documentId, Guid currentUserId);
    Task<ApiResponse<bool>> IncrementDownloadCountAsync(Guid documentId);
    Task<ApiResponse<bool>> UpdateVectorIndexStatusAsync(Guid documentId, bool isIndexed, Guid currentUserId);
}
