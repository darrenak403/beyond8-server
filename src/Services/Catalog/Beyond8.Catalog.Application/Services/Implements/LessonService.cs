using Beyond8.Catalog.Application.Dtos.Lessons;
using Beyond8.Catalog.Application.Mappings.LessonMappings;
using Beyond8.Catalog.Application.Services.Interfaces;
using Beyond8.Catalog.Domain.Entities;
using Beyond8.Catalog.Domain.Repositories.Interfaces;
using Beyond8.Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Beyond8.Catalog.Application.Services.Implements;

public class LessonService(
    ILogger<LessonService> logger,
    IUnitOfWork unitOfWork) : ILessonService
{
    public async Task<ApiResponse<bool>> CallbackHlsAsync(VideoCallbackDto request)
    {
        try
        {
            logger.LogInformation("Callback HLS for original key: {OriginalKey}", request.OriginalKey);
            logger.LogInformation("Transcoding data: {TranscodingData}", JsonSerializer.Serialize(request.TranscodingData));
            var lesson = await unitOfWork.LessonRepository.FindOneAsync(x => string.IsNullOrEmpty(x.VideoOriginalUrl) || x.VideoOriginalUrl.Contains(request.OriginalKey));
            if (lesson == null)
            {
                logger.LogWarning("Lesson not found with original key: {OriginalKey}", request.OriginalKey);
                return ApiResponse<bool>.FailureResponse("Lesson not found");
            }
            lesson.HlsVariants = JsonSerializer.Serialize(request.TranscodingData.Variants);
            await unitOfWork.LessonRepository.UpdateAsync(lesson.Id, lesson);
            await unitOfWork.SaveChangesAsync();
            return ApiResponse<bool>.SuccessResponse(true, "Callback HLS success");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in CallbackHlsAsync");
            return ApiResponse<bool>.FailureResponse("Error in CallbackHlsAsync");
        }
    }
    public async Task<ApiResponse<List<LessonResponse>>> GetLessonsBySectionIdAsync(Guid sectionId, Guid currentUserId)
    {
        try
        {
            // Validate section ownership through course
            var section = await unitOfWork.SectionRepository.AsQueryable()
                .Include(s => s.Course)
                .FirstOrDefaultAsync(s => s.Id == sectionId);

            if (section == null)
            {
                logger.LogWarning("Section not found: {SectionId}", sectionId);
                return ApiResponse<List<LessonResponse>>.FailureResponse("Chương không tồn tại.");
            }

            if (section.Course.InstructorId != currentUserId)
            {
                logger.LogWarning("Access denied for section {SectionId} by user {UserId}", sectionId, currentUserId);
                return ApiResponse<List<LessonResponse>>.FailureResponse("Bạn không có quyền truy cập chương này.");
            }

            var lessons = await unitOfWork.LessonRepository.GetAllAsync(l => l.SectionId == sectionId);
            var orderedLessons = lessons.OrderBy(l => l.OrderIndex).ToList();

            var responses = orderedLessons.Select(l => l.ToResponse()).ToList();

            return ApiResponse<List<LessonResponse>>.SuccessResponse(responses, "Lấy danh sách bài học thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting lessons for section: {SectionId}", sectionId);
            return ApiResponse<List<LessonResponse>>.FailureResponse("Đã xảy ra lỗi khi lấy danh sách bài học.");
        }
    }

    public async Task<ApiResponse<LessonResponse>> GetLessonByIdAsync(Guid lessonId, Guid currentUserId)
    {
        try
        {
            var lesson = await unitOfWork.LessonRepository.AsQueryable()
                .Include(l => l.Section)
                .ThenInclude(s => s.Course)
                .FirstOrDefaultAsync(l => l.Id == lessonId);

            if (lesson == null)
            {
                logger.LogWarning("Lesson not found: {LessonId}", lessonId);
                return ApiResponse<LessonResponse>.FailureResponse("Bài học không tồn tại.");
            }

            if (lesson.Section.Course.InstructorId != currentUserId)
            {
                logger.LogWarning("Access denied for lesson {LessonId} by user {UserId}", lessonId, currentUserId);
                return ApiResponse<LessonResponse>.FailureResponse("Bạn không có quyền truy cập bài học này.");
            }

            return ApiResponse<LessonResponse>.SuccessResponse(lesson.ToResponse(), "Lấy thông tin bài học thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting lesson by id: {LessonId}", lessonId);
            return ApiResponse<LessonResponse>.FailureResponse("Đã xảy ra lỗi khi lấy thông tin bài học.");
        }
    }

    public async Task<ApiResponse<LessonResponse>> CreateLessonAsync(CreateLessonRequest request, Guid currentUserId)
    {
        try
        {
            // Validate section ownership through course
            var section = await unitOfWork.SectionRepository.AsQueryable()
                .Include(s => s.Course)
                .FirstOrDefaultAsync(s => s.Id == request.SectionId);

            if (section == null)
            {
                logger.LogWarning("Section not found: {SectionId}", request.SectionId);
                return ApiResponse<LessonResponse>.FailureResponse("Chương không tồn tại.");
            }

            if (section.Course.InstructorId != currentUserId)
            {
                logger.LogWarning("Access denied for section {SectionId} by user {UserId}", request.SectionId, currentUserId);
                return ApiResponse<LessonResponse>.FailureResponse("Bạn không có quyền tạo bài học trong chương này.");
            }

            // Get max order index for the section
            var maxOrder = await unitOfWork.LessonRepository.AsQueryable()
                .Where(l => l.SectionId == request.SectionId)
                .MaxAsync(l => (int?)l.OrderIndex) ?? 0;

            var lesson = new Lesson
            {
                SectionId = request.SectionId,
                Title = request.Title,
                Description = request.Description,
                Type = request.Type,
                OrderIndex = request.OrderIndex > 0 ? request.OrderIndex : maxOrder + 1,
                IsPreview = request.IsPreview,
                IsPublished = true,
                VideoHlsUrl = request.VideoHlsUrl,
                VideoOriginalUrl = request.VideoOriginalUrl,
                VideoThumbnailUrl = request.VideoThumbnailUrl,
                DurationSeconds = request.DurationSeconds,
                VideoQualities = request.VideoQualities,
                IsDownloadable = request.IsDownloadable,
                TextContent = request.TextContent,
                QuizId = request.QuizId,
                MinCompletionSeconds = request.MinCompletionSeconds,
                RequiredScore = request.RequiredScore
            };

            await unitOfWork.LessonRepository.AddAsync(lesson);
            await unitOfWork.SaveChangesAsync();

            // Update section statistics
            await UpdateSectionStatisticsAsync(request.SectionId);

            logger.LogInformation("Lesson created: {LessonId} for section {SectionId} by user {UserId}", lesson.Id, request.SectionId, currentUserId);

            return ApiResponse<LessonResponse>.SuccessResponse(lesson.ToResponse(), "Tạo bài học thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating lesson for section: {SectionId}", request.SectionId);
            return ApiResponse<LessonResponse>.FailureResponse("Đã xảy ra lỗi khi tạo bài học.");
        }
    }

    public async Task<ApiResponse<LessonResponse>> UpdateLessonAsync(Guid lessonId, UpdateLessonRequest request, Guid currentUserId)
    {
        try
        {
            var lesson = await unitOfWork.LessonRepository.AsQueryable()
                .Include(l => l.Section)
                .ThenInclude(s => s.Course)
                .FirstOrDefaultAsync(l => l.Id == lessonId);

            if (lesson == null)
            {
                logger.LogWarning("Lesson not found: {LessonId}", lessonId);
                return ApiResponse<LessonResponse>.FailureResponse("Bài học không tồn tại.");
            }

            if (lesson.Section.Course.InstructorId != currentUserId)
            {
                logger.LogWarning("Access denied for lesson {LessonId} by user {UserId}", lessonId, currentUserId);
                return ApiResponse<LessonResponse>.FailureResponse("Bạn không có quyền chỉnh sửa bài học này.");
            }

            lesson.Title = request.Title;
            lesson.Description = request.Description;
            lesson.Type = request.Type;
            lesson.IsPreview = request.IsPreview;
            lesson.IsPublished = request.IsPublished;
            lesson.VideoHlsUrl = request.VideoHlsUrl;
            lesson.VideoOriginalUrl = request.VideoOriginalUrl;
            lesson.VideoThumbnailUrl = request.VideoThumbnailUrl;
            lesson.DurationSeconds = request.DurationSeconds;
            lesson.VideoQualities = request.VideoQualities;
            lesson.IsDownloadable = request.IsDownloadable;
            lesson.TextContent = request.TextContent;
            lesson.QuizId = request.QuizId;
            lesson.MinCompletionSeconds = request.MinCompletionSeconds;
            lesson.RequiredScore = request.RequiredScore;

            await unitOfWork.LessonRepository.UpdateAsync(lessonId, lesson);
            await unitOfWork.SaveChangesAsync();

            // Update section statistics
            await UpdateSectionStatisticsAsync(lesson.SectionId);

            logger.LogInformation("Lesson updated: {LessonId} by user {UserId}", lessonId, currentUserId);

            return ApiResponse<LessonResponse>.SuccessResponse(lesson.ToResponse(), "Cập nhật bài học thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating lesson: {LessonId}", lessonId);
            return ApiResponse<LessonResponse>.FailureResponse("Đã xảy ra lỗi khi cập nhật bài học.");
        }
    }

    public async Task<ApiResponse<bool>> DeleteLessonAsync(Guid lessonId, Guid currentUserId)
    {
        try
        {
            var lesson = await unitOfWork.LessonRepository.AsQueryable()
                .Include(l => l.Section)
                .ThenInclude(s => s.Course)
                .FirstOrDefaultAsync(l => l.Id == lessonId);

            if (lesson == null)
            {
                logger.LogWarning("Lesson not found: {LessonId}", lessonId);
                return ApiResponse<bool>.FailureResponse("Bài học không tồn tại.");
            }

            if (lesson.Section.Course.InstructorId != currentUserId)
            {
                logger.LogWarning("Access denied for lesson {LessonId} by user {UserId}", lessonId, currentUserId);
                return ApiResponse<bool>.FailureResponse("Bạn không có quyền xóa bài học này.");
            }

            await unitOfWork.LessonRepository.DeleteAsync(lessonId);
            await unitOfWork.SaveChangesAsync();

            // Update section statistics
            await UpdateSectionStatisticsAsync(lesson.SectionId);

            logger.LogInformation("Lesson deleted: {LessonId} by user {UserId}", lessonId, currentUserId);

            return ApiResponse<bool>.SuccessResponse(true, "Xóa bài học thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting lesson: {LessonId}", lessonId);
            return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi xóa bài học.");
        }
    }

    public async Task<ApiResponse<bool>> ReorderLessonsAsync(Guid sectionId, List<ReorderLessonRequest> requests, Guid currentUserId)
    {
        try
        {
            // Validate section ownership through course
            var section = await unitOfWork.SectionRepository.AsQueryable()
                .Include(s => s.Course)
                .FirstOrDefaultAsync(s => s.Id == sectionId);

            if (section == null)
            {
                logger.LogWarning("Section not found: {SectionId}", sectionId);
                return ApiResponse<bool>.FailureResponse("Chương không tồn tại.");
            }

            if (section.Course.InstructorId != currentUserId)
            {
                logger.LogWarning("Access denied for section {SectionId} by user {UserId}", sectionId, currentUserId);
                return ApiResponse<bool>.FailureResponse("Bạn không có quyền truy cập chương này.");
            }

            // Validate all lesson IDs belong to the section
            var lessonIds = requests.Select(r => r.LessonId).ToList();
            var lessons = await unitOfWork.LessonRepository.GetAllAsync(l => l.SectionId == sectionId && lessonIds.Contains(l.Id));

            if (lessons.Count != requests.Count)
            {
                logger.LogWarning("Some lessons not found or don't belong to section {SectionId}", sectionId);
                return ApiResponse<bool>.FailureResponse("Một số bài học không tồn tại hoặc không thuộc chương này.");
            }

            // Update order indexes
            foreach (var request in requests)
            {
                var lesson = lessons.First(l => l.Id == request.LessonId);
                lesson.OrderIndex = request.NewOrderIndex;
                await unitOfWork.LessonRepository.UpdateAsync(lesson.Id, lesson);
            }

            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Lessons reordered for section {SectionId} by user {UserId}", sectionId, currentUserId);

            return ApiResponse<bool>.SuccessResponse(true, "Sắp xếp lại bài học thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error reordering lessons for section: {SectionId}", sectionId);
            return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi sắp xếp lại bài học.");
        }
    }

    private async Task UpdateSectionStatisticsAsync(Guid sectionId)
    {
        try
        {
            var section = await unitOfWork.SectionRepository.FindOneAsync(s => s.Id == sectionId);
            if (section == null) return;

            var lessons = await unitOfWork.LessonRepository.GetAllAsync(l => l.SectionId == sectionId);

            section.TotalLessons = lessons.Count;
            section.TotalDurationMinutes = lessons
                .Where(l => l.DurationSeconds.HasValue)
                .Sum(l => l.DurationSeconds.GetValueOrDefault(0)) / 60;

            await unitOfWork.SectionRepository.UpdateAsync(sectionId, section);
            await unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating section statistics for section: {SectionId}", sectionId);
        }
    }
}
