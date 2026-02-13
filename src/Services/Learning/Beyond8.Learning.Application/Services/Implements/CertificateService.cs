using Beyond8.Common.Events.Learning;
using Beyond8.Common.Utilities;
using Beyond8.Learning.Application.Clients.Catalog;
using Beyond8.Learning.Application.Clients.Identity;
using Beyond8.Learning.Application.Dtos.Certificates;
using Beyond8.Learning.Application.Mappings;
using Beyond8.Learning.Application.Services.Interfaces;
using Beyond8.Learning.Domain.Entities;
using Beyond8.Learning.Domain.Repositories.Interfaces;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Beyond8.Learning.Application.Services.Implements;

public class CertificateService(
    ILogger<CertificateService> logger,
    IUnitOfWork unitOfWork,
    IIdentityClient identityClient,
    ICatalogClient catalogClient,
    IPublishEndpoint publishEndpoint) : ICertificateService
{
    private const decimal DefaultQuizAverageMinPercent = 70m;
    private const decimal DefaultAssignmentAverageMinPercent = 50m;

    public async Task TryIssueCertificateIfEligibleAsync(Guid enrollmentId)
    {
        try
        {
            var enrollment = await unitOfWork.EnrollmentRepository.AsQueryable()
                .Include(e => e.LessonProgresses)
                .Include(e => e.SectionProgresses)
                .FirstOrDefaultAsync(e => e.Id == enrollmentId && e.DeletedAt == null);

            if (enrollment == null || enrollment.CertificateId.HasValue)
                return;

            if (enrollment.TotalLessons <= 0 || enrollment.CompletedLessons < enrollment.TotalLessons)
                return;

            var config = await unitOfWork.CourseCertificateEligibilityConfigRepository
                .FindOneAsync(c => c.CourseId == enrollment.CourseId);
            var quizMin = config?.QuizAverageMinPercent ?? DefaultQuizAverageMinPercent;
            var assignmentMin = config?.AssignmentAverageMinPercent ?? DefaultAssignmentAverageMinPercent;

            var quizLessons = enrollment.LessonProgresses.Where(lp => (lp.QuizAttempts ?? 0) > 0).ToList();
            if (quizLessons.Count > 0)
            {
                var quizScores = quizLessons.Where(lp => lp.QuizBestScore.HasValue).Select(lp => lp.QuizBestScore!.Value).ToList();
                if (quizScores.Count == 0 || quizScores.Average() < quizMin)
                    return;
            }

            var assignmentSections = enrollment.SectionProgresses.Where(sp => sp.AssignmentSubmitted).ToList();
            if (assignmentSections.Count > 0)
            {
                var graded = assignmentSections.Where(sp => sp.AssignmentGrade.HasValue).ToList();
                if (graded.Count < assignmentSections.Count)
                    return;
                if (graded.Select(sp => sp.AssignmentGrade!.Value).Average() < assignmentMin)
                    return;
            }

            var now = DateTime.UtcNow;
            var cert = enrollment.ToCertificateEntity(now);

            await unitOfWork.CertificateRepository.AddAsync(cert);
            enrollment.CertificateId = cert.Id;
            enrollment.CertificateIssuedAt = now;
            await unitOfWork.EnrollmentRepository.UpdateAsync(enrollment.Id, enrollment);
            await unitOfWork.SaveChangesAsync();

            string? userEmail = null;
            string? userFullName = null;
            var userResponse = await identityClient.GetUserByIdAsync(enrollment.UserId);
            if (userResponse.IsSuccess && userResponse.Data != null)
            {
                userEmail = userResponse.Data.Email;
                userFullName = userResponse.Data.FullName;
            }

            await publishEndpoint.Publish(new CourseCompletedEvent(
                enrollment.Id,
                enrollment.UserId,
                enrollment.CourseId,
                enrollment.CourseTitle,
                enrollment.CompletedAt ?? now,
                CertificateId: cert.Id,
                UserEmail: userEmail,
                UserFullName: userFullName));

            logger.LogInformation(
                "Certificate issued: EnrollmentId {EnrollmentId}, CertificateId {CertificateId}, UserId {UserId}, CourseId {CourseId}",
                enrollmentId, cert.Id, enrollment.UserId, enrollment.CourseId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error trying to issue certificate for enrollment {EnrollmentId}", enrollmentId);
        }
    }

    public async Task<ApiResponse<CertificateVerificationResponse>> GetByVerificationHashAsync(string hash)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(hash))
                return ApiResponse<CertificateVerificationResponse>.FailureResponse("Mã chứng chỉ không hợp lệ.");

            var cert = await unitOfWork.CertificateRepository.FindOneAsync(c =>
                c.VerificationHash == hash.Trim());

            if (cert == null)
            {
                logger.LogDebug("Certificate not found for hash: {Hash}", hash);
                return ApiResponse<CertificateVerificationResponse>.FailureResponse("Không tìm thấy chứng chỉ với mã này.");
            }

            return ApiResponse<CertificateVerificationResponse>.SuccessResponse(
                cert.ToVerificationResponse(),
                "Xác thực chứng chỉ thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error verifying certificate for hash: {Hash}", hash);
            return ApiResponse<CertificateVerificationResponse>.FailureResponse("Đã xảy ra lỗi khi kiểm tra chứng chỉ.");
        }
    }

    public async Task<ApiResponse<List<CertificateSimpleResponse>>> GetMyCertificatesAsync(Guid userId)
    {
        try
        {
            var list = await unitOfWork.CertificateRepository.GetAllAsync(c => c.UserId == userId);
            var items = list.OrderByDescending(c => c.IssuedDate).Select(c => c.ToSimpleResponse()).ToList();
            return ApiResponse<List<CertificateSimpleResponse>>.SuccessResponse(
                items,
                "Lấy danh sách chứng chỉ thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting my certificates for user {UserId}", userId);
            return ApiResponse<List<CertificateSimpleResponse>>.FailureResponse("Đã xảy ra lỗi khi lấy danh sách chứng chỉ.");
        }
    }

    public async Task<ApiResponse<CertificateDetailResponse>> GetByIdAsync(Guid id, Guid userId)
    {
        try
        {
            var cert = await unitOfWork.CertificateRepository.FindOneAsync(c => c.Id == id);
            if (cert == null)
                return ApiResponse<CertificateDetailResponse>.FailureResponse("Chứng chỉ không tồn tại.");
            if (cert.UserId != userId)
                return ApiResponse<CertificateDetailResponse>.FailureResponse("Bạn không có quyền xem chứng chỉ này.");
            return ApiResponse<CertificateDetailResponse>.SuccessResponse(
                cert.ToDetailResponse(),
                "Lấy chi tiết chứng chỉ thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting certificate {Id} for user {UserId}", id, userId);
            return ApiResponse<CertificateDetailResponse>.FailureResponse("Đã xảy ra lỗi khi lấy chi tiết chứng chỉ.");
        }
    }

    public async Task<ApiResponse<CertificateEligibilityConfigResponse>> GetCertificateEligibilityConfigAsync(Guid courseId, Guid userId)
    {
        try
        {
            var ownership = await EnsureCourseInstructorAsync(courseId, userId);
            if (!ownership.IsSuccess)
                return ApiResponse<CertificateEligibilityConfigResponse>.FailureResponse(ownership.ErrorMessage!);

            var config = await unitOfWork.CourseCertificateEligibilityConfigRepository
                .FindOneAsync(c => c.CourseId == courseId);
            var response = new CertificateEligibilityConfigResponse
            {
                CourseId = courseId,
                QuizAverageMinPercent = config?.QuizAverageMinPercent,
                AssignmentAverageMinPercent = config?.AssignmentAverageMinPercent
            };
            return ApiResponse<CertificateEligibilityConfigResponse>.SuccessResponse(
                response,
                "Lấy cấu hình điều kiện cấp chứng chỉ thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting certificate eligibility config for course {CourseId}", courseId);
            return ApiResponse<CertificateEligibilityConfigResponse>.FailureResponse("Đã xảy ra lỗi khi lấy cấu hình.");
        }
    }

    public async Task<ApiResponse<CertificateEligibilityConfigResponse>> UpdateCertificateEligibilityConfigAsync(Guid courseId, UpdateCertificateEligibilityConfigRequest request, Guid userId)
    {
        try
        {
            var ownership = await EnsureCourseInstructorAsync(courseId, userId);
            if (!ownership.IsSuccess)
                return ApiResponse<CertificateEligibilityConfigResponse>.FailureResponse(ownership.ErrorMessage!);

            var config = await unitOfWork.CourseCertificateEligibilityConfigRepository
                .FindOneAsync(c => c.CourseId == courseId);
            var now = DateTime.UtcNow;

            if (config != null)
            {
                if (request.QuizAverageMinPercent.HasValue)
                    config.QuizAverageMinPercent = request.QuizAverageMinPercent;
                if (request.AssignmentAverageMinPercent.HasValue)
                    config.AssignmentAverageMinPercent = request.AssignmentAverageMinPercent;
                config.UpdatedAt = now;
                config.UpdatedBy = userId;
                await unitOfWork.CourseCertificateEligibilityConfigRepository.UpdateAsync(config.Id, config);
            }
            else
            {
                config = new CourseCertificateEligibilityConfig
                {
                    CourseId = courseId,
                    QuizAverageMinPercent = request.QuizAverageMinPercent,
                    AssignmentAverageMinPercent = request.AssignmentAverageMinPercent,
                    CreatedAt = now,
                    CreatedBy = userId
                };
                await unitOfWork.CourseCertificateEligibilityConfigRepository.AddAsync(config);
            }

            await unitOfWork.SaveChangesAsync();

            var response = new CertificateEligibilityConfigResponse
            {
                CourseId = courseId,
                QuizAverageMinPercent = config.QuizAverageMinPercent,
                AssignmentAverageMinPercent = config.AssignmentAverageMinPercent
            };
            logger.LogInformation(
                "Certificate eligibility config updated: CourseId {CourseId}, QuizMin {QuizMin}, AssignmentMin {AssignmentMin}",
                courseId, config.QuizAverageMinPercent, config.AssignmentAverageMinPercent);
            return ApiResponse<CertificateEligibilityConfigResponse>.SuccessResponse(
                response,
                "Cập nhật cấu hình điều kiện cấp chứng chỉ thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating certificate eligibility config for course {CourseId}", courseId);
            return ApiResponse<CertificateEligibilityConfigResponse>.FailureResponse("Đã xảy ra lỗi khi cập nhật cấu hình.");
        }
    }

    private async Task<(bool IsSuccess, string? ErrorMessage)> EnsureCourseInstructorAsync(Guid courseId, Guid userId)
    {
        var structureResult = await catalogClient.GetCourseStructureAsync(courseId);
        if (!structureResult.IsSuccess || structureResult.Data == null)
            return (false, structureResult.Message ?? "Khóa học không tồn tại.");
        if (structureResult.Data.InstructorId != userId)
            return (false, "Bạn không có quyền cấu hình điều kiện cấp chứng chỉ cho khóa học này.");
        return (true, null);
    }
}
