using Beyond8.Catalog.Application.Dtos.Lessons;
using Beyond8.Common.Utilities;

namespace Beyond8.Catalog.Application.Services.Interfaces;

public interface ILessonService
{
    Task<ApiResponse<bool>> CallbackHlsAsync(VideoCallbackDto request);
    Task<ApiResponse<List<LessonResponse>>> GetLessonsBySectionIdAsync(Guid sectionId, Guid currentUserId);
    Task<ApiResponse<LessonResponse>> GetLessonByIdAsync(Guid lessonId, Guid currentUserId);
    Task<ApiResponse<LessonResponse>> CreateLessonAsync(CreateLessonRequest request, Guid currentUserId);
    Task<ApiResponse<LessonResponse>> UpdateLessonAsync(Guid lessonId, UpdateLessonRequest request, Guid currentUserId);
    Task<ApiResponse<bool>> DeleteLessonAsync(Guid lessonId, Guid currentUserId);
}
