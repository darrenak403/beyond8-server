using Beyond8.Common.Utilities;
using Beyond8.Learning.Application.Clients.Catalog;
using Beyond8.Learning.Application.Dtos.Enrollments;
using Beyond8.Learning.Application.Mappings;
using Beyond8.Learning.Application.Services.Interfaces;
using Beyond8.Learning.Domain.Entities;
using Beyond8.Learning.Domain.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace Beyond8.Learning.Application.Services.Implements;

public class EnrollmentService(
    ILogger<EnrollmentService> logger,
    IUnitOfWork unitOfWork,
    ICatalogClient catalogClient) : IEnrollmentService
{
    private const int CourseStatusPublished = 4; // Catalog.CourseStatus.Published

    public async Task<ApiResponse<EnrollmentResponse>> EnrollFreeAsync(Guid userId, EnrollFreeRequest request)
    {
        var courseId = request.CourseId;

        var structureResult = await catalogClient.GetCourseStructureAsync(courseId);
        if (!structureResult.IsSuccess || structureResult.Data == null)
        {
            logger.LogWarning("Failed to get course structure for {CourseId}: {Message}", courseId, structureResult.Message);
            return ApiResponse<EnrollmentResponse>.FailureResponse(
                structureResult.Message ?? "Không thể lấy thông tin khóa học.");
        }

        var structure = structureResult.Data;

        // TODO: Lấy thêm discount của courseId từ bảng coupon (Sales Service) để check có giảm giá hay không
        // (vd: coupon 100% → cho phép enroll free dù Price != 0). Hiện tại chỉ cho enroll khi Price == 0.
        if (structure.Price != 0)
        {
            logger.LogWarning("User {UserId} attempted to enroll free in paid course {CourseId}", userId, courseId);
            return ApiResponse<EnrollmentResponse>.FailureResponse("Khóa học không miễn phí. Vui lòng thanh toán để đăng ký.");
        }

        if (structure.Status != CourseStatusPublished)
        {
            return ApiResponse<EnrollmentResponse>.FailureResponse("Khóa học chưa được xuất bản hoặc không khả dụng.");
        }

        var existing = await unitOfWork.EnrollmentRepository.FindOneAsync(e =>
            e.UserId == userId && e.CourseId == courseId && e.DeletedAt == null);
        if (existing != null)
        {
            return ApiResponse<EnrollmentResponse>.FailureResponse("Bạn đã đăng ký khóa học này rồi.");
        }

        var totalLessons = structure.TotalLessons;
        if (totalLessons <= 0 && structure.Sections.Count > 0)
        {
            totalLessons = structure.Sections.Sum(s => s.Lessons.Count);
        }

        var enrollment = structure.ToEnrollmentEntity(userId, courseId, totalLessons, pricePaid: 0);
        await unitOfWork.EnrollmentRepository.AddAsync(enrollment);

        foreach (var section in structure.Sections.OrderBy(s => s.Order))
        {
            var sectionProgress = section.ToSectionProgressEntity(userId, courseId, enrollment.Id);
            await unitOfWork.SectionProgressRepository.AddAsync(sectionProgress);
        }

        foreach (var section in structure.Sections.OrderBy(s => s.Order))
        {
            foreach (var lesson in section.Lessons.OrderBy(l => l.Order))
            {
                var lessonProgress = lesson.ToLessonProgressEntity(userId, courseId, enrollment.Id);
                await unitOfWork.LessonProgressRepository.AddAsync(lessonProgress);
            }
        }

        await unitOfWork.SaveChangesAsync();

        logger.LogInformation("User {UserId} enrolled in free course {CourseId}, EnrollmentId {EnrollmentId}",
            userId, courseId, enrollment.Id);

        return ApiResponse<EnrollmentResponse>.SuccessResponse(
            enrollment.ToResponse(),
            "Đăng ký khóa học miễn phí thành công.");
    }

    public async Task<ApiResponse<bool>> IsUserEnrolledInCourseAsync(Guid userId, Guid courseId)
    {
        var enrollment = await unitOfWork.EnrollmentRepository.FindOneAsync(e =>
            e.UserId == userId && e.CourseId == courseId && e.DeletedAt == null);
        return ApiResponse<bool>.SuccessResponse(enrollment != null, enrollment != null ? "Đã đăng ký khóa học." : "Chưa đăng ký khóa học.");
    }
}
