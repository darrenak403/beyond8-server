using Beyond8.Common.Utilities;
using Beyond8.Learning.Application.Dtos.Progress;

namespace Beyond8.Learning.Application.Services.Interfaces;

public interface IProgressService
{
    Task<ApiResponse<LessonProgressResponse>> UpdateLessonProgressAsync(Guid lessonId, Guid userId, LessonProgressHeartbeatRequest request);

    Task<ApiResponse<LessonProgressResponse>> GetLessonProgressAsync(Guid enrollmentId, Guid lessonId, Guid userId);
}
