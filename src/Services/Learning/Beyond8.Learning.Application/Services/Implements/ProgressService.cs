using Beyond8.Common.Utilities;
using Beyond8.Learning.Application.Clients.Catalog;
using Beyond8.Learning.Application.Dtos.Progress;
using Beyond8.Learning.Application.Helpers;
using Beyond8.Learning.Application.Mappings;
using Beyond8.Learning.Application.Services.Interfaces;
using Beyond8.Learning.Domain.Enums;
using Beyond8.Learning.Domain.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace Beyond8.Learning.Application.Services.Implements;

public class ProgressService(
    ILogger<ProgressService> logger,
    IUnitOfWork unitOfWork,
    ICertificateService certificateService,
    ICatalogClient catalogClient) : IProgressService
{
    public async Task<ApiResponse<LessonProgressResponse>> UpdateLessonProgressAsync(Guid lessonId, Guid userId, LessonProgressHeartbeatRequest request)
    {
        try
        {
            var lp = await unitOfWork.LessonProgressRepository.FindOneAsync(l =>
                l.LessonId == lessonId && l.UserId == userId);

            if (lp == null)
            {
                logger.LogWarning("LessonProgress not found for LessonId {LessonId}, UserId {UserId}", lessonId, userId);
                return ApiResponse<LessonProgressResponse>.FailureResponse("Tiến độ bài học không tồn tại.");
            }

            var now = DateTime.UtcNow;
            lp.LastAccessedAt = now;

            if (request.LastPositionSeconds.HasValue)
            {
                var position = Math.Max(0, request.LastPositionSeconds.Value);
                lp.LastPositionSeconds = lp.TotalDurationSeconds > 0
                    ? Math.Min(position, lp.TotalDurationSeconds)
                    : position;
                lp.WatchPercent = lp.TotalDurationSeconds > 0
                    ? Math.Min(100, (decimal)lp.LastPositionSeconds * 100 / lp.TotalDurationSeconds)
                    : 0;
            }

            if (request.MarkComplete)
            {
                lp.Status = LessonProgressStatus.Completed;
                lp.CompletedAt = now;
                lp.IsManuallyCompleted = true;
                lp.WatchPercent = 100;
                if (lp.StartedAt == null)
                    lp.StartedAt = now;
            }
            else
            {
                if (lp.Status == LessonProgressStatus.NotStarted)
                {
                    lp.Status = LessonProgressStatus.InProgress;
                    if (lp.StartedAt == null)
                        lp.StartedAt = now;
                }
                if (lp.Status != LessonProgressStatus.Completed && lp.WatchPercent >= 100)
                {
                    lp.Status = LessonProgressStatus.Completed;
                    lp.CompletedAt = lp.CompletedAt ?? now;
                }
            }

            await unitOfWork.LessonProgressRepository.UpdateAsync(lp.Id, lp);

            var enrollment = await unitOfWork.EnrollmentRepository.FindOneAsync(e =>
                e.Id == lp.EnrollmentId && e.DeletedAt == null);
            if (enrollment != null)
            {
                var completedCount = (int)await unitOfWork.LessonProgressRepository.CountAsync(l =>
                    l.EnrollmentId == lp.EnrollmentId &&
                    EnrollmentProgressHelper.IsCompletedOrFailed(l.Status));
                EnrollmentProgressHelper.ApplyProgressToEnrollment(enrollment, completedCount, now, lessonId, now);
                await unitOfWork.EnrollmentRepository.UpdateAsync(enrollment.Id, enrollment);
            }

            await unitOfWork.SaveChangesAsync();

            await certificateService.TryIssueCertificateIfEligibleAsync(lp.EnrollmentId);

            logger.LogInformation("Lesson progress updated: LessonId {LessonId}, UserId {UserId}, Status {Status}",
                lessonId, userId, lp.Status);

            return ApiResponse<LessonProgressResponse>.SuccessResponse(lp.ToResponse(), "Cập nhật tiến độ bài học thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating lesson progress: LessonId {LessonId}, UserId {UserId}", lessonId, userId);
            return ApiResponse<LessonProgressResponse>.FailureResponse("Đã xảy ra lỗi khi cập nhật tiến độ bài học.");
        }
    }

    public async Task<ApiResponse<LessonProgressResponse>> GetLessonProgressAsync(Guid enrollmentId, Guid lessonId, Guid userId)
    {
        try
        {
            var enrollment = await unitOfWork.EnrollmentRepository.FindOneAsync(e =>
                e.Id == enrollmentId && e.UserId == userId && e.DeletedAt == null);
            if (enrollment == null)
            {
                logger.LogWarning("Enrollment not found or access denied: {EnrollmentId}, UserId {UserId}", enrollmentId, userId);
                return ApiResponse<LessonProgressResponse>.FailureResponse("Khóa học đã đăng ký không tồn tại hoặc không có quyền truy cập.");
            }

            var lp = await unitOfWork.LessonProgressRepository.FindOneAsync(l =>
                l.EnrollmentId == enrollmentId && l.LessonId == lessonId);
            if (lp == null)
            {
                logger.LogWarning("LessonProgress not found: EnrollmentId {EnrollmentId}, LessonId {LessonId}", enrollmentId, lessonId);
                return ApiResponse<LessonProgressResponse>.FailureResponse("Tiến độ bài học không tồn tại.");
            }

            return ApiResponse<LessonProgressResponse>.SuccessResponse(lp.ToResponse(), "Lấy tiến độ bài học thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting lesson progress: EnrollmentId {EnrollmentId}, LessonId {LessonId}", enrollmentId, lessonId);
            return ApiResponse<LessonProgressResponse>.FailureResponse("Đã xảy ra lỗi khi lấy tiến độ bài học.");
        }
    }

    public async Task<ApiResponse<CurriculumProgressResponse>> GetCurriculumProgressByEnrollmentIdAsync(Guid enrollmentId, Guid userId)
    {
        try
        {
            var enrollment = await unitOfWork.EnrollmentRepository.FindOneAsync(e =>
                e.Id == enrollmentId && e.UserId == userId && e.DeletedAt == null);
            if (enrollment == null)
            {
                logger.LogWarning("Enrollment not found or access denied: {EnrollmentId}, UserId {UserId}", enrollmentId, userId);
                return ApiResponse<CurriculumProgressResponse>.FailureResponse("Khóa học đã đăng ký không tồn tại hoặc không có quyền truy cập.");
            }

            var structureResult = await catalogClient.GetCourseStructureAsync(enrollment.CourseId);
            if (!structureResult.IsSuccess || structureResult.Data == null)
            {
                logger.LogWarning("Failed to get course structure for CourseId {CourseId}", enrollment.CourseId);
                return ApiResponse<CurriculumProgressResponse>.FailureResponse(
                    structureResult.Message ?? "Không thể lấy khung chương trình khóa học.");
            }

            var structure = structureResult.Data;
            var lessonProgressList = (await unitOfWork.LessonProgressRepository.GetAllAsync(lp =>
                lp.EnrollmentId == enrollmentId)).ToList();
            var lessonProgressDict = lessonProgressList.ToDictionary(lp => lp.LessonId);

            var sectionProgressList = (await unitOfWork.SectionProgressRepository.GetAllAsync(sp =>
                sp.EnrollmentId == enrollmentId)).ToList();
            var sectionProgressDict = sectionProgressList.ToDictionary(sp => sp.SectionId);

            var response = enrollment.ToCurriculumProgressResponse(structure, lessonProgressDict, sectionProgressDict);

            return ApiResponse<CurriculumProgressResponse>.SuccessResponse(response, "Lấy tiến độ khung chương trình thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting curriculum progress: EnrollmentId {EnrollmentId}, UserId {UserId}", enrollmentId, userId);
            return ApiResponse<CurriculumProgressResponse>.FailureResponse("Đã xảy ra lỗi khi lấy tiến độ khung chương trình.");
        }
    }
}
