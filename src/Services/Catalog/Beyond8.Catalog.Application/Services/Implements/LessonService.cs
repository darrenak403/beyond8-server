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
                return ApiResponse<bool>.FailureResponse("Bài học không tồn tại.");
            }
            lesson.Video!.HlsVariants = JsonSerializer.Serialize(request.TranscodingData.Variants);
            await unitOfWork.LessonVideoRepository.UpdateAsync(lesson.Video.Id, lesson.Video);
            await unitOfWork.SaveChangesAsync();
            return ApiResponse<bool>.SuccessResponse(true, "Cập nhật HLS thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in CallbackHlsAsync");
            return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi cập nhật HLS.");
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

            var lesson = request.ToEntity();
            lesson.OrderIndex = await GetNextOrderIndexForSectionAsync(request.SectionId);

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

            var lesson = request.ToEntity();
            lesson.OrderIndex = await GetNextOrderIndexForSectionAsync(request.SectionId);

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

            var nextOrderIndex = await GetNextOrderIndexForSectionAsync(request.SectionId);
            var lesson = request.ToEntity(nextOrderIndex);

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

            await RemoveOtherLessonTypeEntitiesAsync(lesson, LessonType.Video);

            lesson.Type = LessonType.Video;
            await unitOfWork.LessonRepository.UpdateAsync(lessonId, lesson);
            await unitOfWork.SaveChangesAsync();

            // Update section statistics
            await UpdateSectionStatisticsAsync(lesson.SectionId);

            logger.LogInformation("Video lesson updated: {LessonId} by user {UserId}", lessonId, currentUserId);

            var refreshed = await GetLessonWithIncludesAsync(lessonId);
            return ApiResponse<LessonResponse>.SuccessResponse(refreshed!.ToResponse(), "Cập nhật bài học video thành công.");
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

            await RemoveOtherLessonTypeEntitiesAsync(lesson, LessonType.Text);

            lesson.Type = LessonType.Text;
            await unitOfWork.LessonRepository.UpdateAsync(lessonId, lesson);
            await unitOfWork.SaveChangesAsync();

            // Update section statistics
            await UpdateSectionStatisticsAsync(lesson.SectionId);

            logger.LogInformation("Text lesson updated: {LessonId} by user {UserId}", lessonId, currentUserId);

            var refreshed = await GetLessonWithIncludesAsync(lessonId);
            return ApiResponse<LessonResponse>.SuccessResponse(refreshed!.ToResponse(), "Cập nhật bài học văn bản thành công.");
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

            await RemoveOtherLessonTypeEntitiesAsync(lesson, LessonType.Quiz);

            lesson.Type = LessonType.Quiz;
            await unitOfWork.LessonRepository.UpdateAsync(lessonId, lesson);
            await unitOfWork.SaveChangesAsync();

            // Update section statistics
            await UpdateSectionStatisticsAsync(lesson.SectionId);

            logger.LogInformation("Quiz lesson updated: {LessonId} by user {UserId}", lessonId, currentUserId);

            var refreshed = await GetLessonWithIncludesAsync(lessonId);
            return ApiResponse<LessonResponse>.SuccessResponse(refreshed!.ToResponse(), "Cập nhật bài học quiz thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating quiz lesson: {LessonId}", lessonId);
            return ApiResponse<LessonResponse>.FailureResponse("Đã xảy ra lỗi khi cập nhật bài học quiz.");
        }
    }

    private async Task<int> GetNextOrderIndexForSectionAsync(Guid sectionId)
    {
        var maxOrder = await unitOfWork.LessonRepository.AsQueryable()
            .Where(l => l.SectionId == sectionId)
            .MaxAsync(l => (int?)l.OrderIndex) ?? 0;
        return maxOrder + 1;
    }

    private async Task RemoveOtherLessonTypeEntitiesAsync(Lesson lesson, LessonType keepType)
    {
        if (keepType != LessonType.Video && lesson.Video != null)
            await unitOfWork.LessonVideoRepository.DeleteAsync(lesson.Video.Id);
        if (keepType != LessonType.Text && lesson.Text != null)
            await unitOfWork.LessonTextRepository.DeleteAsync(lesson.Text.Id);
        if (keepType != LessonType.Quiz && lesson.Quiz != null)
            await unitOfWork.LessonQuizRepository.DeleteAsync(lesson.Quiz.Id);
    }

    private async Task<Lesson?> GetLessonWithIncludesAsync(Guid lessonId)
    {
        return await unitOfWork.LessonRepository.AsQueryable()
            .Include(l => l.Video)
            .Include(l => l.Text)
            .Include(l => l.Quiz)
            .FirstOrDefaultAsync(l => l.Id == lessonId);
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

            var lessons = await unitOfWork.LessonRepository.AsQueryable()
                .Where(l => l.SectionId == sectionId)
                .Include(l => l.Video)
                .ToListAsync();

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

    public async Task<ApiResponse<bool>> ReorderLessonInSectionAsync(ReorderLessonInSectionRequest request, Guid currentUserId)
    {
        try
        {
            // Kiểm tra lesson có tồn tại không
            var lesson = await unitOfWork.LessonRepository.FindOneAsync(l => l.Id == request.LessonId);
            if (lesson == null)
                return ApiResponse<bool>.FailureResponse("Bài học không tồn tại.");

            // Check quyền sở hữu bài học
            var validationResult = await CheckSectionOwnershipAsync(lesson.SectionId, currentUserId);
            if (!validationResult.IsValid)
                return ApiResponse<bool>.FailureResponse(validationResult.ErrorMessage ?? "Không có quyền truy cập bài học.");

            var oldIndex = lesson.OrderIndex;
            var sectionId = lesson.SectionId;

            // No change needed
            if (oldIndex == request.NewOrderIndex)
                return ApiResponse<bool>.SuccessResponse(true, "Sắp xếp bài học thành công.");

            await unitOfWork.BeginTransactionAsync();

            try
            {
                if (request.NewOrderIndex > oldIndex)
                {
                    // Di chuyển xuống: giảm orderIndex cho các mục nằm giữa oldIndex và newIndex
                    await unitOfWork.LessonRepository.AsQueryable()
                        .Where(l => l.SectionId == sectionId && l.OrderIndex > oldIndex && l.OrderIndex <= request.NewOrderIndex)
                        .ExecuteUpdateAsync(l => l.SetProperty(x => x.OrderIndex, x => x.OrderIndex - 1));
                }
                else
                {
                    // Di chuyển lên: tăng orderIndex cho các mục nằm giữa newIndex và oldIndex
                    await unitOfWork.LessonRepository.AsQueryable()
                        .Where(l => l.SectionId == sectionId && l.OrderIndex >= request.NewOrderIndex && l.OrderIndex < oldIndex)
                        .ExecuteUpdateAsync(l => l.SetProperty(x => x.OrderIndex, x => x.OrderIndex + 1));
                }

                // Cập nhật bài học mục tiêu
                lesson.OrderIndex = request.NewOrderIndex;
                await unitOfWork.LessonRepository.UpdateAsync(request.LessonId, lesson);

                await unitOfWork.CommitTransactionAsync();

                logger.LogInformation("Lesson reordered in section: {LessonId} from index {OldIndex} to index {NewIndex} by user {UserId}",
                    request.LessonId, oldIndex, request.NewOrderIndex, currentUserId);

                return ApiResponse<bool>.SuccessResponse(true, "Sắp xếp bài học trong section thành công.");
            }
            catch
            {
                await unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error reordering lesson in section: {LessonId}", request.LessonId);
            return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi sắp xếp bài học trong section.");
        }
    }

    public async Task<ApiResponse<bool>> MoveLessonToSectionAsync(MoveLessonToSectionRequest request, Guid currentUserId)
    {
        try
        {
            // Kiểm tra lesson có tồn tại không
            var lesson = await unitOfWork.LessonRepository.FindOneAsync(l => l.Id == request.LessonId);
            if (lesson == null)
                return ApiResponse<bool>.FailureResponse("Bài học không tồn tại.");

            // Check quyền sở hữu bài học
            var validationResult = await CheckSectionOwnershipAsync(lesson.SectionId, currentUserId);
            if (!validationResult.IsValid)
                return ApiResponse<bool>.FailureResponse(validationResult.ErrorMessage ?? "Không có quyền truy cập bài học.");

            // Check quyền sở hữu section đích
            var targetValidation = await CheckSectionOwnershipAsync(request.NewSectionId, currentUserId);
            if (!targetValidation.IsValid)
                return ApiResponse<bool>.FailureResponse(targetValidation.ErrorMessage ?? "Không có quyền truy cập section đích.");

            var oldSectionId = lesson.SectionId;
            var oldIndex = lesson.OrderIndex;

            // No change needed
            if (oldSectionId == request.NewSectionId && oldIndex == request.NewOrderIndex)
                return ApiResponse<bool>.SuccessResponse(true, "Di chuyển bài học thành công.");

            await unitOfWork.BeginTransactionAsync();

            try
            {
                // 1. Dịch chuyển các mục trong section cũ (đóng khoảng trống)
                await unitOfWork.LessonRepository.AsQueryable()
                    .Where(l => l.SectionId == oldSectionId && l.OrderIndex > oldIndex)
                    .ExecuteUpdateAsync(l => l.SetProperty(x => x.OrderIndex, x => x.OrderIndex - 1));

                // 2. Dịch chuyển các mục trong section mới (tạo khoảng trống)
                await unitOfWork.LessonRepository.AsQueryable()
                    .Where(l => l.SectionId == request.NewSectionId && l.OrderIndex >= request.NewOrderIndex)
                    .ExecuteUpdateAsync(l => l.SetProperty(x => x.OrderIndex, x => x.OrderIndex + 1));

                // 3. Cập nhật bài học mục tiêu
                lesson.SectionId = request.NewSectionId;
                lesson.OrderIndex = request.NewOrderIndex;
                await unitOfWork.LessonRepository.UpdateAsync(request.LessonId, lesson);

                await unitOfWork.CommitTransactionAsync();

                logger.LogInformation("Lesson moved: {LessonId} from section {OldSectionId} index {OldIndex} to section {NewSectionId} index {NewIndex} by user {UserId}",
                    request.LessonId, oldSectionId, oldIndex, request.NewSectionId, request.NewOrderIndex, currentUserId);

                return ApiResponse<bool>.SuccessResponse(true, "Di chuyển bài học sang section khác thành công.");
            }
            catch
            {
                await unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error moving lesson to section: {LessonId}", request.LessonId);
            return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi di chuyển bài học sang section khác.");
        }
    }

    public async Task<ApiResponse<bool>> ReorderSectionAsync(ReorderSectionRequest request, Guid currentUserId)
    {
        try
        {
            // Kiểm tra section có tồn tại không
            var section = await unitOfWork.SectionRepository.FindOneAsync(s => s.Id == request.SectionId);
            if (section == null)
                return ApiResponse<bool>.FailureResponse("Chương không tồn tại.");

            // Check quyền sở hữu section
            var validationResult = await CheckSectionOwnershipAsync(request.SectionId, currentUserId);
            if (!validationResult.IsValid)
                return ApiResponse<bool>.FailureResponse(validationResult.ErrorMessage ?? "Không có quyền truy cập chương.");

            var oldIndex = section.OrderIndex;
            var courseId = section.CourseId;

            // No change needed
            if (oldIndex == request.NewOrderIndex)
                return ApiResponse<bool>.SuccessResponse(true, "Sắp xếp chương thành công.");

            await unitOfWork.BeginTransactionAsync();

            try
            {
                if (request.NewOrderIndex > oldIndex)
                {
                    // Di chuyển xuống: giảm orderIndex cho các mục nằm giữa oldIndex và newIndex
                    await unitOfWork.SectionRepository.AsQueryable()
                        .Where(s => s.CourseId == courseId && s.OrderIndex > oldIndex && s.OrderIndex <= request.NewOrderIndex)
                        .ExecuteUpdateAsync(s => s.SetProperty(x => x.OrderIndex, x => x.OrderIndex - 1));
                }
                else
                {
                    // Di chuyển lên: tăng orderIndex cho các mục nằm giữa newIndex và oldIndex
                    await unitOfWork.SectionRepository.AsQueryable()
                        .Where(s => s.CourseId == courseId && s.OrderIndex >= request.NewOrderIndex && s.OrderIndex < oldIndex)
                        .ExecuteUpdateAsync(s => s.SetProperty(x => x.OrderIndex, x => x.OrderIndex + 1));
                }

                // Cập nhật section mục tiêu
                section.OrderIndex = request.NewOrderIndex;
                await unitOfWork.SectionRepository.UpdateAsync(request.SectionId, section);

                await unitOfWork.CommitTransactionAsync();

                logger.LogInformation("Section reordered: {SectionId} from index {OldIndex} to index {NewIndex} by user {UserId}",
                    request.SectionId, oldIndex, request.NewOrderIndex, currentUserId);

                return ApiResponse<bool>.SuccessResponse(true, "Sắp xếp chương thành công.");
            }
            catch
            {
                await unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error reordering section: {SectionId}", request.SectionId);
            return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi sắp xếp chương.");
        }
    }
}
