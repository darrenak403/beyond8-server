using System.Linq;
using System.Text.Json;
using Beyond8.Catalog.Application.Dtos.Lessons;
using Beyond8.Catalog.Application.Mappings.LessonMappings;
using Beyond8.Catalog.Application.Services.Interfaces;
using Beyond8.Catalog.Domain.Entities;
using Beyond8.Catalog.Domain.Enums;
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
            var lesson = await unitOfWork.LessonRepository.AsQueryable()
                .Include(l => l.Video)
                .FirstOrDefaultAsync(x => x.Video != null && (string.IsNullOrEmpty(x.Video.VideoOriginalUrl) || x.Video.VideoOriginalUrl.Contains(request.OriginalKey)));
            if (lesson == null)
            {
                logger.LogWarning("Lesson not found with original key: {OriginalKey}", request.OriginalKey);
                return ApiResponse<bool>.FailureResponse("Lesson not found");
            }
            lesson.Video!.HlsVariants = JsonSerializer.Serialize(request.TranscodingData.Variants);
            await unitOfWork.LessonVideoRepository.UpdateAsync(lesson.Video.Id, lesson.Video);
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
            var validationResult = await CheckSectionOwnershipAsync(sectionId, currentUserId);
            if (!validationResult.IsValid)
                return ApiResponse<List<LessonResponse>>.FailureResponse(validationResult.ErrorMessage!);

            var lessons = await unitOfWork.LessonRepository.AsQueryable()
                .Where(l => l.SectionId == sectionId)
                .OrderBy(l => l.OrderIndex)
                .Include(l => l.Video)
                .Include(l => l.Text)
                .Include(l => l.Quiz)
                .ToListAsync();

            var responses = lessons.Select(l => l.ToResponse()).ToList();

            return ApiResponse<List<LessonResponse>>.SuccessResponse(
                responses,
                "Lấy danh sách bài học thành công.");
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
            // Validate lesson ownership
            var (isValid, lesson, errorMessage) = await CheckLessonOwnershipAsync(lessonId, currentUserId);
            if (!isValid)
                return ApiResponse<LessonResponse>.FailureResponse(errorMessage!);

            return ApiResponse<LessonResponse>.SuccessResponse(lesson!.ToResponse(), "Lấy thông tin bài học thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting lesson by id: {LessonId}", lessonId);
            return ApiResponse<LessonResponse>.FailureResponse("Đã xảy ra lỗi khi lấy thông tin bài học.");
        }
    }

    public async Task<ApiResponse<bool>> DeleteLessonAsync(Guid lessonId, Guid currentUserId)
    {
        try
        {
            // Validate lesson ownership
            var (isValid, lesson, errorMessage) = await CheckLessonOwnershipAsync(lessonId, currentUserId);
            if (!isValid)
                return ApiResponse<bool>.FailureResponse(errorMessage!);

            // Delete specific lesson type entities first
            if (lesson!.Video != null) await unitOfWork.LessonVideoRepository.DeleteAsync(lesson.Video.Id);
            if (lesson.Text != null) await unitOfWork.LessonTextRepository.DeleteAsync(lesson.Text.Id);
            if (lesson.Quiz != null) await unitOfWork.LessonQuizRepository.DeleteAsync(lesson.Quiz.Id);

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

    // New overload methods for specific lesson types
    public async Task<ApiResponse<LessonResponse>> CreateVideoLessonAsync(CreateVideoLessonRequest request, Guid currentUserId)
    {
        try
        {
            // Validate section ownership through course
            var validationResult = await CheckSectionOwnershipAsync(request.SectionId, currentUserId);
            if (!validationResult.IsValid)
                return ApiResponse<LessonResponse>.FailureResponse(validationResult.ErrorMessage!);

            var maxOrder = await unitOfWork.LessonRepository.AsQueryable()
                .Where(l => l.SectionId == request.SectionId)
                .MaxAsync(l => (int?)l.OrderIndex) ?? 0;

            var lesson = request.ToEntity();
            lesson.OrderIndex = maxOrder + 1;

            await unitOfWork.LessonRepository.AddAsync(lesson);

            var videoEntity = request.ToVideoEntity(lesson.Id);
            await unitOfWork.LessonVideoRepository.AddAsync(videoEntity);
            await unitOfWork.SaveChangesAsync();

            // Update section statistics
            await UpdateSectionStatisticsAsync(request.SectionId);

            logger.LogInformation("Video lesson created: {LessonId} for section {SectionId} by user {UserId}", lesson.Id, request.SectionId, currentUserId);

            return ApiResponse<LessonResponse>.SuccessResponse(lesson.ToResponse(), "Tạo bài học video thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating video lesson for section: {SectionId}", request.SectionId);
            return ApiResponse<LessonResponse>.FailureResponse("Đã xảy ra lỗi khi tạo bài học video.");
        }
    }

    public async Task<ApiResponse<LessonResponse>> CreateTextLessonAsync(CreateTextLessonRequest request, Guid currentUserId)
    {
        try
        {
            // Validate section ownership through course
            var validationResult = await CheckSectionOwnershipAsync(request.SectionId, currentUserId);
            if (!validationResult.IsValid)
                return ApiResponse<LessonResponse>.FailureResponse(validationResult.ErrorMessage!);

            var maxOrder = await unitOfWork.LessonRepository.AsQueryable()
                .Where(l => l.SectionId == request.SectionId)
                .MaxAsync(l => (int?)l.OrderIndex) ?? 0;

            var lesson = request.ToEntity();
            lesson.OrderIndex = maxOrder + 1;

            await unitOfWork.LessonRepository.AddAsync(lesson);

            var textEntity = request.ToTextEntity(lesson.Id);
            await unitOfWork.LessonTextRepository.AddAsync(textEntity);
            await unitOfWork.SaveChangesAsync();

            // Update section statistics
            await UpdateSectionStatisticsAsync(request.SectionId);

            logger.LogInformation("Text lesson created: {LessonId} for section {SectionId} by user {UserId}", lesson.Id, request.SectionId, currentUserId);

            return ApiResponse<LessonResponse>.SuccessResponse(lesson.ToResponse(), "Tạo bài học văn bản thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating text lesson for section: {SectionId}", request.SectionId);
            return ApiResponse<LessonResponse>.FailureResponse("Đã xảy ra lỗi khi tạo bài học văn bản.");
        }
    }

    public async Task<ApiResponse<LessonResponse>> CreateQuizLessonAsync(CreateQuizLessonRequest request, Guid currentUserId)
    {
        try
        {
            // Validate section ownership through course
            var validationResult = await CheckSectionOwnershipAsync(request.SectionId, currentUserId);
            if (!validationResult.IsValid)
                return ApiResponse<LessonResponse>.FailureResponse(validationResult.ErrorMessage!);

            var maxOrder = await unitOfWork.LessonRepository.AsQueryable()
                .Where(l => l.SectionId == request.SectionId)
                .MaxAsync(l => (int?)l.OrderIndex) ?? 0;

            var lesson = request.ToEntity();
            lesson.OrderIndex = maxOrder + 1;

            await unitOfWork.LessonRepository.AddAsync(lesson);

            var quizEntity = request.ToQuizEntity(lesson.Id);
            await unitOfWork.LessonQuizRepository.AddAsync(quizEntity);
            await unitOfWork.SaveChangesAsync();

            // Update section statistics
            await UpdateSectionStatisticsAsync(request.SectionId);

            logger.LogInformation("Quiz lesson created: {LessonId} for section {SectionId} by user {UserId}", lesson.Id, request.SectionId, currentUserId);

            return ApiResponse<LessonResponse>.SuccessResponse(lesson.ToResponse(), "Tạo bài học quiz thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating quiz lesson for section: {SectionId}", request.SectionId);
            return ApiResponse<LessonResponse>.FailureResponse("Đã xảy ra lỗi khi tạo bài học quiz.");
        }
    }

    public async Task<ApiResponse<LessonResponse>> UpdateVideoLessonAsync(Guid lessonId, UpdateVideoLessonRequest request, Guid currentUserId)
    {
        try
        {
            // Validate lesson ownership
            var (isValid, lesson, errorMessage) = await CheckLessonOwnershipAsync(lessonId, currentUserId);
            if (!isValid)
                return ApiResponse<LessonResponse>.FailureResponse(errorMessage!);

            // Update base lesson properties
            lesson!.UpdateFrom(request);

            // Update or create video entity
            if (lesson!.Video != null)
            {
                lesson.Video.UpdateVideoFrom(request);
                await unitOfWork.LessonVideoRepository.UpdateAsync(lesson.Video.Id!, lesson.Video!);
            }
            else
            {
                var videoEntity = request.ToVideoEntity(lesson.Id);
                await unitOfWork.LessonVideoRepository.AddAsync(videoEntity);
            }

            // Remove other types if they exist
            if (lesson.Text != null) await unitOfWork.LessonTextRepository.DeleteAsync(lesson.Text.Id);
            if (lesson.Quiz != null) await unitOfWork.LessonQuizRepository.DeleteAsync(lesson.Quiz.Id);

            lesson.Type = LessonType.Video;
            await unitOfWork.LessonRepository.UpdateAsync(lessonId, lesson);
            await unitOfWork.SaveChangesAsync();

            // Update section statistics
            await UpdateSectionStatisticsAsync(lesson.SectionId);

            logger.LogInformation("Video lesson updated: {LessonId} by user {UserId}", lessonId, currentUserId);

            return ApiResponse<LessonResponse>.SuccessResponse(lesson.ToResponse(), "Cập nhật bài học video thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating video lesson: {LessonId}", lessonId);
            return ApiResponse<LessonResponse>.FailureResponse("Đã xảy ra lỗi khi cập nhật bài học video.");
        }
    }

    public async Task<ApiResponse<LessonResponse>> UpdateTextLessonAsync(Guid lessonId, UpdateTextLessonRequest request, Guid currentUserId)
    {
        try
        {
            // Validate lesson ownership
            var (isValid, lesson, errorMessage) = await CheckLessonOwnershipAsync(lessonId, currentUserId);
            if (!isValid)
                return ApiResponse<LessonResponse>.FailureResponse(errorMessage!);

            // Update base lesson properties
            lesson!.UpdateFrom(request);

            // Update or create text entity
            if (lesson!.Text != null)
            {
                lesson.Text.UpdateTextFrom(request);
                await unitOfWork.LessonTextRepository.UpdateAsync(lesson.Text.Id!, lesson.Text!);
            }
            else
            {
                var textEntity = request.ToTextEntity(lesson.Id);
                await unitOfWork.LessonTextRepository.AddAsync(textEntity);
            }

            // Remove other types if they exist
            if (lesson!.Video != null) await unitOfWork.LessonVideoRepository.DeleteAsync(lesson.Video.Id);
            if (lesson!.Quiz != null) await unitOfWork.LessonQuizRepository.DeleteAsync(lesson.Quiz.Id);

            lesson.Type = LessonType.Text;
            await unitOfWork.LessonRepository.UpdateAsync(lessonId, lesson);
            await unitOfWork.SaveChangesAsync();

            // Update section statistics
            await UpdateSectionStatisticsAsync(lesson.SectionId);

            logger.LogInformation("Text lesson updated: {LessonId} by user {UserId}", lessonId, currentUserId);

            return ApiResponse<LessonResponse>.SuccessResponse(lesson.ToResponse(), "Cập nhật bài học văn bản thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating text lesson: {LessonId}", lessonId);
            return ApiResponse<LessonResponse>.FailureResponse("Đã xảy ra lỗi khi cập nhật bài học văn bản.");
        }
    }

    public async Task<ApiResponse<LessonResponse>> UpdateQuizLessonAsync(Guid lessonId, UpdateQuizLessonRequest request, Guid currentUserId)
    {
        try
        {
            // Validate lesson ownership
            var (isValid, lesson, errorMessage) = await CheckLessonOwnershipAsync(lessonId, currentUserId);
            if (!isValid)
                return ApiResponse<LessonResponse>.FailureResponse(errorMessage!);

            // Update base lesson properties
            lesson!.UpdateFrom(request);

            // Update or create quiz entity
            if (lesson!.Quiz != null)
            {
                lesson.Quiz.UpdateQuizFrom(request);
                await unitOfWork.LessonQuizRepository.UpdateAsync(lesson.Quiz.Id!, lesson.Quiz!);
            }
            else
            {
                var quizEntity = request.ToQuizEntity(lesson.Id);
                await unitOfWork.LessonQuizRepository.AddAsync(quizEntity);
            }

            // Remove other types if they exist
            if (lesson.Video != null) await unitOfWork.LessonVideoRepository.DeleteAsync(lesson.Video.Id);
            if (lesson.Text != null) await unitOfWork.LessonTextRepository.DeleteAsync(lesson.Text.Id);

            lesson.Type = LessonType.Quiz;
            await unitOfWork.LessonRepository.UpdateAsync(lessonId, lesson);
            await unitOfWork.SaveChangesAsync();

            // Update section statistics
            await UpdateSectionStatisticsAsync(lesson.SectionId);

            logger.LogInformation("Quiz lesson updated: {LessonId} by user {UserId}", lessonId, currentUserId);

            return ApiResponse<LessonResponse>.SuccessResponse(lesson.ToResponse(), "Cập nhật bài học quiz thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating quiz lesson: {LessonId}", lessonId);
            return ApiResponse<LessonResponse>.FailureResponse("Đã xảy ra lỗi khi cập nhật bài học quiz.");
        }
    }

    private async Task<(bool IsValid, string? ErrorMessage)> CheckSectionOwnershipAsync(Guid sectionId, Guid currentUserId)
    {
        var section = await unitOfWork.SectionRepository.AsQueryable()
            .Include(s => s.Course)
            .FirstOrDefaultAsync(s => s.Id == sectionId);

        if (section == null)
        {
            logger.LogWarning("Section not found: {SectionId}", sectionId);
            return (false, "Chương không tồn tại.");
        }

        if (section.Course.InstructorId != currentUserId)
        {
            logger.LogWarning("Access denied for section {SectionId} by user {UserId}", sectionId, currentUserId);
            return (false, "Bạn không có quyền truy cập chương này.");
        }

        return (true, null);
    }

    private async Task<(bool IsValid, Lesson? Lesson, string? ErrorMessage)> CheckLessonOwnershipAsync(Guid lessonId, Guid currentUserId)
    {
        var lesson = await unitOfWork.LessonRepository.AsQueryable()
            .Include(l => l.Section)
            .ThenInclude(s => s.Course)
            .Include(l => l.Video)
            .Include(l => l.Text)
            .Include(l => l.Quiz)
            .FirstOrDefaultAsync(l => l.Id == lessonId);

        if (lesson == null)
        {
            logger.LogWarning("Lesson not found: {LessonId}", lessonId);
            return (false, null, "Bài học không tồn tại.");
        }

        if (lesson.Section.Course.InstructorId != currentUserId)
        {
            logger.LogWarning("Access denied for lesson {LessonId} by user {UserId}", lessonId, currentUserId);
            return (false, null, "Bạn không có quyền truy cập bài học này.");
        }

        return (true, lesson, null);
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
                .Where(l => l.Video?.DurationSeconds.HasValue == true)
                .Sum(l => l.Video!.DurationSeconds.GetValueOrDefault(0)) / 60;

            await unitOfWork.SectionRepository.UpdateAsync(sectionId, section);
            await unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating section statistics for section: {SectionId}", sectionId);
        }
    }

    public async Task<ApiResponse<bool>> ChangeQuizForLessonAsync(Guid lessonId, Guid? quizId, Guid currentUserId)
    {
        try
        {
            // Validate lesson ownership
            var (isValid, lesson, errorMessage) = await CheckLessonOwnershipAsync(lessonId, currentUserId);
            if (!isValid)
                return ApiResponse<bool>.FailureResponse(errorMessage!);

            if (lesson!.Quiz != null)
            {
                lesson.Quiz.QuizId = quizId;
                await unitOfWork.LessonQuizRepository.UpdateAsync(lesson.Quiz.Id!, lesson.Quiz!);
                await unitOfWork.SaveChangesAsync();

                logger.LogInformation("Quiz ID updated for lesson: {LessonId} by user {UserId}", lessonId, currentUserId);
                return ApiResponse<bool>.SuccessResponse(true, "Cập nhật Quiz ID cho bài học thành công.");
            }
            else
            {
                logger.LogWarning("Lesson {LessonId} does not have a quiz entity to update", lessonId);
                return ApiResponse<bool>.FailureResponse("Bài học này không có quiz để cập nhật.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error changing quiz for lesson: {LessonId}", lessonId);
            return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi thay đổi quiz cho bài học.");
        }
    }

    public async Task<ApiResponse<bool>> SwitchLessonActivationAsync(Guid lessonId, bool isPublished, Guid currentUserId)
    {
        try
        {
            // Validate lesson ownership
            var (isValid, lesson, errorMessage) = await CheckLessonOwnershipAsync(lessonId, currentUserId);
            if (!isValid)
                return ApiResponse<bool>.FailureResponse(errorMessage!);

            lesson!.IsPublished = isPublished;
            await unitOfWork.LessonRepository.UpdateAsync(lessonId, lesson);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Lesson activation switched: {LessonId} to {IsPublished} by user {UserId}", lessonId, isPublished, currentUserId);

            return ApiResponse<bool>.SuccessResponse(true, "Chuyển đổi trạng thái kích hoạt bài học thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error switching lesson activation: {LessonId}", lessonId);
            return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi chuyển đổi trạng thái kích hoạt bài học.");
        }
    }
}
