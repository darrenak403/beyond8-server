using Beyond8.Catalog.Application.Dtos.Lessons;
using Beyond8.Common.Utilities;

namespace Beyond8.Catalog.Application.Services.Interfaces;

public interface ILessonService
{
    Task<ApiResponse<bool>> CallbackHlsAsync(VideoCallbackDto request);
    Task<ApiResponse<List<LessonResponse>>> GetLessonsBySectionIdAsync(Guid sectionId, Guid currentUserId);
    Task<ApiResponse<LessonResponse>> GetLessonByIdAsync(Guid lessonId, Guid currentUserId);
    Task<ApiResponse<bool>> DeleteLessonAsync(Guid lessonId, Guid currentUserId);

    // New overloads for specific lesson types
    Task<ApiResponse<LessonResponse>> CreateVideoLessonAsync(CreateVideoLessonRequest request, Guid currentUserId);
    Task<ApiResponse<LessonResponse>> CreateTextLessonAsync(CreateTextLessonRequest request, Guid currentUserId);
    Task<ApiResponse<LessonResponse>> CreateQuizLessonAsync(CreateQuizLessonRequest request, Guid currentUserId);

    Task<ApiResponse<LessonResponse>> UpdateVideoLessonAsync(Guid lessonId, UpdateVideoLessonRequest request, Guid currentUserId);
    Task<ApiResponse<LessonResponse>> UpdateTextLessonAsync(Guid lessonId, UpdateTextLessonRequest request, Guid currentUserId);
    Task<ApiResponse<LessonResponse>> UpdateQuizLessonAsync(Guid lessonId, UpdateQuizLessonRequest request, Guid currentUserId);
    Task<ApiResponse<bool>> UpdateQuizForLessonAsync(Guid lessonId, Guid? quizId, Guid currentUserId);
    Task<ApiResponse<bool>> SwitchLessonActivationAsync(Guid lessonId, bool isPublished, Guid currentUserId);
    Task<ApiResponse<bool>> ReorderLessonInSectionAsync(ReorderLessonInSectionRequest request, Guid currentUserId);
    Task<ApiResponse<bool>> MoveLessonToSectionAsync(MoveLessonToSectionRequest request, Guid currentUserId);
    Task<ApiResponse<bool>> ReorderSectionAsync(ReorderSectionRequest request, Guid currentUserId);
}
