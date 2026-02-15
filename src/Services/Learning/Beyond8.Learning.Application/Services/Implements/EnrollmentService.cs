using Beyond8.Common.Events.Learning;
using Beyond8.Common.Events.Cache;
using Beyond8.Common.Utilities;
using Beyond8.Learning.Application.Clients.Catalog;
using Beyond8.Learning.Application.Dtos.Enrollments;
using Beyond8.Learning.Application.Helpers;
using Beyond8.Learning.Application.Mappings;
using Beyond8.Learning.Application.Services.Interfaces;
using Beyond8.Learning.Domain.Entities;
using Beyond8.Learning.Domain.Enums;
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

        var result = enrollment != null;

        return ApiResponse<bool>.SuccessResponse(result, result ? "Đã đăng ký khóa học." : "Chưa đăng ký khóa học.", metadata: new { EnrollmentId = enrollment?.Id });
    }

    public async Task<ApiResponse<bool>> HasCertificateForCourseAsync(Guid studentId, Guid courseId)
    {
        var enrollment = await unitOfWork.EnrollmentRepository.FindOneAsync(e =>
            e.UserId == studentId && e.CourseId == courseId && e.DeletedAt == null);

        var hasCertificate = enrollment?.CertificateId != null;

        return ApiResponse<bool>.SuccessResponse(hasCertificate,
            hasCertificate ? "Học sinh đã được cấp certificate cho khóa học này." : "Chưa có certificate.");
    }

    public async Task<ApiResponse<bool>> EnrollPaidCoursesAsync(Guid userId, List<Guid> courseIds, Guid orderId)
    {
        if (courseIds == null || courseIds.Count == 0)
        {
            logger.LogWarning("No courses provided for enrollment from order {OrderId}", orderId);
            return ApiResponse<bool>.FailureResponse("Không có khóa học nào để đăng ký");
        }

        var successCount = 0;
        var errors = new List<string>();

        foreach (var courseId in courseIds)
        {
            try
            {
                var structureResult = await catalogClient.GetCourseStructureAsync(courseId);
                if (!structureResult.IsSuccess || structureResult.Data == null)
                {
                    var error = $"Không thể lấy thông tin khóa học {courseId}: {structureResult.Message}";
                    logger.LogWarning(error);
                    errors.Add(error);
                    continue;
                }

                var structure = structureResult.Data;

                // Check if already enrolled
                var existing = await unitOfWork.EnrollmentRepository.FindOneAsync(e =>
                    e.UserId == userId && e.CourseId == courseId && e.DeletedAt == null);
                if (existing != null)
                {
                    logger.LogInformation("User {UserId} already enrolled in course {CourseId} from order {OrderId}",
                        userId, courseId, orderId);
                    successCount++;
                    continue;
                }

                var totalLessons = structure.TotalLessons;
                if (totalLessons <= 0 && structure.Sections.Count > 0)
                {
                    totalLessons = structure.Sections.Sum(s => s.Lessons.Count);
                }

                var enrollment = new Enrollment
                {
                    UserId = userId,
                    CourseId = courseId,
                    CourseTitle = structure.Title,
                    CourseThumbnailUrl = structure.ThumbnailUrl,
                    Slug = structure.Slug,
                    InstructorId = structure.InstructorId,
                    InstructorName = structure.InstructorName,
                    PricePaid = structure.FinalPrice, // Price paid for this course
                    Status = EnrollmentStatus.Active,
                    TotalLessons = totalLessons,
                    EnrolledAt = DateTime.UtcNow
                };

                await unitOfWork.EnrollmentRepository.AddAsync(enrollment);

                // Create section progresses
                foreach (var section in structure.Sections.OrderBy(s => s.Order))
                {
                    var sectionProgress = section.ToSectionProgressEntity(userId, courseId, enrollment.Id);
                    await unitOfWork.SectionProgressRepository.AddAsync(sectionProgress);

                    // Create lesson progresses
                    foreach (var lesson in section.Lessons.OrderBy(l => l.Order))
                    {
                        var lessonProgress = lesson.ToLessonProgressEntity(userId, courseId, enrollment.Id);
                        await unitOfWork.LessonProgressRepository.AddAsync(lessonProgress);
                    }
                }

                await unitOfWork.SaveChangesAsync();

                // Publish events
                var totalStudents = await unitOfWork.EnrollmentRepository.CountActiveByCourseIdAsync(courseId);
                await Task.WhenAll(
                    publishEndpoint.Publish(new CourseEnrollmentCountChangedEvent(
                        courseId, totalStudents, DateTime.UtcNow, structure.InstructorId, 1)),

                    publishEndpoint.Publish(new CacheInvalidateEvent($"enrolled_courses:{userId}"))
                );

                logger.LogInformation("Successfully enrolled user {UserId} in paid course {CourseId} from order {OrderId}",
                    userId, courseId, orderId);
                successCount++;
            }
            catch (Exception ex)
            {
                var error = $"Lỗi khi đăng ký khóa học {courseId}: {ex.Message}";
                logger.LogError(ex, error);
                errors.Add(error);
            }
        }

        if (successCount == courseIds.Count)
        {
            return ApiResponse<bool>.SuccessResponse(true,
                $"Đăng ký thành công {successCount} khóa học từ đơn hàng {orderId}");
        }
        else if (successCount > 0)
        {
            return ApiResponse<bool>.SuccessResponse(true,
                $"Đăng ký thành công {successCount}/{courseIds.Count} khóa học từ đơn hàng {orderId}. Lỗi: {string.Join("; ", errors)}");
        }
        else
        {
            return ApiResponse<bool>.FailureResponse($"Không thể đăng ký khóa học nào. Lỗi: {string.Join("; ", errors)}");
        }
    }

    public async Task<ApiResponse<List<EnrollmentSimpleResponse>>> GetEnrolledCoursesAsync(Guid userId)
    {
        try
        {
            var enrollments = (await unitOfWork.EnrollmentRepository.GetEnrolledCoursesAsync(userId)).ToList();
            if (enrollments.Count == 0)
                return ApiResponse<List<EnrollmentSimpleResponse>>.SuccessResponse([], "Lấy danh sách khóa học đã đăng ký thành công.");

            var enrollmentIds = enrollments.Select(e => e.Id).ToList();
            var completedLessonProgress = await unitOfWork.LessonProgressRepository.GetAllAsync(lp =>
                enrollmentIds.Contains(lp.EnrollmentId) &&
                (lp.Status == LessonProgressStatus.Completed || lp.Status == LessonProgressStatus.Failed));
            var completedCountByEnrollmentId = completedLessonProgress
                .GroupBy(lp => lp.EnrollmentId)
                .ToDictionary(g => g.Key, g => g.Count());

            var responses = enrollments.Select(e =>
            {
                var completedCount = completedCountByEnrollmentId.GetValueOrDefault(e.Id, 0);
                var progressPercent = EnrollmentProgressHelper.CalculateProgressPercent(completedCount, e.TotalLessons);
                return e.ToSimpleResponse(progressPercent);
            }).ToList();

            return ApiResponse<List<EnrollmentSimpleResponse>>.SuccessResponse(responses, "Lấy danh sách khóa học đã đăng ký thành công.");
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
