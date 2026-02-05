using Beyond8.Common.Utilities;
using Beyond8.Learning.Application.Dtos.Certificates;
using Beyond8.Learning.Application.Mappings;
using Beyond8.Learning.Application.Services.Interfaces;
using Beyond8.Learning.Domain.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Beyond8.Learning.Application.Services.Implements;

public class CertificateService(
    ILogger<CertificateService> logger,
    IUnitOfWork unitOfWork) : ICertificateService
{
    private const decimal QuizAverageThresholdPercent = 70m;
    private const decimal AssignmentAverageThreshold = 5m;

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

            var quizLessons = enrollment.LessonProgresses.Where(lp => (lp.QuizAttempts ?? 0) > 0).ToList();
            if (quizLessons.Count > 0)
            {
                var quizScores = quizLessons.Where(lp => lp.QuizBestScore.HasValue).Select(lp => lp.QuizBestScore!.Value).ToList();
                if (quizScores.Count == 0 || quizScores.Average() < QuizAverageThresholdPercent)
                    return;
            }

            var assignmentSections = enrollment.SectionProgresses.Where(sp => sp.AssignmentSubmitted).ToList();
            if (assignmentSections.Count > 0)
            {
                var graded = assignmentSections.Where(sp => sp.AssignmentGrade.HasValue).ToList();
                if (graded.Count < assignmentSections.Count)
                    return;
                if (graded.Select(sp => sp.AssignmentGrade!.Value).Average() < AssignmentAverageThreshold)
                    return;
            }

            var now = DateTime.UtcNow;
            var cert = enrollment.ToCertificateEntity(now);

            await unitOfWork.CertificateRepository.AddAsync(cert);
            enrollment.CertificateId = cert.Id;
            enrollment.CertificateIssuedAt = now;
            await unitOfWork.EnrollmentRepository.UpdateAsync(enrollment.Id, enrollment);
            await unitOfWork.SaveChangesAsync();

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
}
