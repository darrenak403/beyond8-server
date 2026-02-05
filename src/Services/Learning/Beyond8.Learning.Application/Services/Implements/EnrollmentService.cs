using Beyond8.Common.Events.Learning;
using Beyond8.Common.Events.Cache;
using Beyond8.Common.Utilities;
using Beyond8.Learning.Application.Clients.Catalog;
using Beyond8.Learning.Application.Dtos.Enrollments;
using Beyond8.Learning.Application.Mappings;
using Beyond8.Learning.Application.Services.Interfaces;
using Beyond8.Learning.Domain.Repositories.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Learning.Application.Services.Implements;

public class EnrollmentService(
    ILogger<EnrollmentService> logger,
    IUnitOfWork unitOfWork,
    ICatalogClient catalogClient,
    IPublishEndpoint publishEndpoint) : IEnrollmentService
{
    private const int CourseStatusPublished = 4;

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

        if (structure.FinalPrice != 0)
        {
            logger.LogWarning("User {UserId} attempted to enroll free in paid course {CourseId}", userId, courseId);
            return ApiResponse<EnrollmentResponse>.FailureResponse("Khóa học không miễn phí. Vui lòng thanh toán để đăng ký.");
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

        var totalStudents = await unitOfWork.EnrollmentRepository.CountActiveByCourseIdAsync(courseId);

        await Task.WhenAll(
            publishEndpoint.Publish(new CourseEnrollmentCountChangedEvent(
                courseId, totalStudents, DateTime.UtcNow, structure.InstructorId, 1)),

            publishEndpoint.Publish(new CacheInvalidateEvent($"enrolled_courses:{userId}")),

            publishEndpoint.Publish(new FreeEnrollmentOrderRequestEvent(
                userId, courseId, enrollment.Id, 0))
        );

        logger.LogInformation("User {UserId} enrolled in free course {CourseId}, EnrollmentId {EnrollmentId}",
            userId, courseId, enrollment.Id);

        return ApiResponse<EnrollmentResponse>.SuccessResponse(
            enrollment.ToResponse(),
            "Đăng ký khóa học miễn phí thành công.");
    }

    public async Task<ApiResponse<List<Guid>>> GetEnrolledCourseIdsAsync(Guid userId)
    {
        try
        {
            var enrolledCourseIds = await unitOfWork.EnrollmentRepository.GetEnrolledCourseIdsAsync(userId);
            logger.LogInformation("Retrieved {Count} enrolled course IDs for user {UserId}", enrolledCourseIds.Count, userId);
            return ApiResponse<List<Guid>>.SuccessResponse(enrolledCourseIds, "Lấy danh sách khóa học đã đăng ký thành công.");

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi khi lấy danh sách khóa học đã đăng ký cho người dùng {UserId}", userId);
            return ApiResponse<List<Guid>>.FailureResponse("Lấy danh sách khóa học đã đăng ký thất bại.");
        }
    }

    public async Task<ApiResponse<bool>> IsUserEnrolledInCourseAsync(Guid userId, Guid courseId)
    {
        var enrollment = await unitOfWork.EnrollmentRepository.FindOneAsync(e =>
            e.UserId == userId && e.CourseId == courseId && e.DeletedAt == null);
        return ApiResponse<bool>.SuccessResponse(enrollment != null, enrollment != null ? "Đã đăng ký khóa học." : "Chưa đăng ký khóa học.");
    }

    public async Task<ApiResponse<List<EnrollmentSimpleResponse>>> GetEnrolledCoursesAsync(Guid userId)
    {
        try
        {
            var enrollments = await unitOfWork.EnrollmentRepository.GetEnrolledCoursesAsync(userId);
            return ApiResponse<List<EnrollmentSimpleResponse>>.SuccessResponse(enrollments.Select(e => e.ToSimpleResponse()).ToList(), "Lấy danh sách khóa học đã đăng ký thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi khi lấy danh sách khóa học đã đăng ký cho người dùng {UserId}", userId);
            return ApiResponse<List<EnrollmentSimpleResponse>>.FailureResponse("Lấy danh sách khóa học đã đăng ký thất bại.");
        }
    }

    public async Task<ApiResponse<EnrollmentResponse>> GetEnrollmentByIdAsync(Guid id, Guid userId)
    {
        try
        {
            var enrollment = await unitOfWork.EnrollmentRepository.FindOneAsync(e => e.Id == id && e.UserId == userId && e.DeletedAt == null);
            if (enrollment == null)
            {
                return ApiResponse<EnrollmentResponse>.FailureResponse("Khóa học đã đăng ký không tồn tại.");
            }
            return ApiResponse<EnrollmentResponse>.SuccessResponse(enrollment.ToResponse(), "Lấy thông tin khóa học đã đăng ký thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi khi lấy thông tin khóa học đã đăng ký theo ID {Id} cho người dùng {UserId}", id, userId);
            return ApiResponse<EnrollmentResponse>.FailureResponse("Lấy thông tin khóa học đã đăng ký thất bại.");
        }
    }
}
