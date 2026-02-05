using Beyond8.Common.Events.Learning;
using Beyond8.Common.Utilities;
using Beyond8.Learning.Application.Dtos.CourseReview;
using Beyond8.Learning.Application.Mappings;
using Beyond8.Learning.Application.Services.Interfaces;
using Beyond8.Learning.Domain.Repositories.Interfaces;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Beyond8.Learning.Application.Services.Implements;

public class CourseReviewService(
    ILogger<CourseReviewService> logger,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint) : ICourseReviewService
{
    public async Task<ApiResponse<CourseReviewResponse>> CreateCourseReviewAsync(CreateCourseReviewRequest request, Guid userId)
    {
        try
        {
            var enrollment = await unitOfWork.EnrollmentRepository.FindOneAsync(e =>
                e.Id == request.EnrollmentId && e.DeletedAt == null);
            if (enrollment == null)
            {
                logger.LogWarning("Enrollment not found: {EnrollmentId}", request.EnrollmentId);
                return ApiResponse<CourseReviewResponse>.FailureResponse("Khóa học đã đăng ký không tồn tại.");
            }

            // if (enrollment.TotalLessons <= 0 || enrollment.CompletedLessons < enrollment.TotalLessons)
            // {
            //     logger.LogWarning("User {UserId} has not completed course {CourseId} (progress {Completed}/{Total})",
            //         userId, request.CourseId, enrollment.CompletedLessons, enrollment.TotalLessons);
            //     return ApiResponse<CourseReviewResponse>.FailureResponse("Bạn cần hoàn thành khóa học (100% tiến độ) trước khi đánh giá.");
            // }

            var isCourseReviewed = await unitOfWork.CourseReviewRepository.IsCourseReviewedAsync(request.CourseId, userId);
            if (isCourseReviewed)
            {
                logger.LogWarning("User {UserId} has already reviewed course {CourseId}", userId, request.CourseId);
                return ApiResponse<CourseReviewResponse>.FailureResponse("Bạn đã đánh giá khóa học này rồi.");
            }

            var courseReview = request.ToEntity(userId);
            await unitOfWork.CourseReviewRepository.AddAsync(courseReview);
            await unitOfWork.SaveChangesAsync();

            var (courseAvg, courseTotal) = await GetCourseReviewStatsAsync(request.CourseId);
            var instructorAvg = await GetInstructorAvgRatingAsync(enrollment.InstructorId);
            await publishEndpoint.Publish(new CourseRatingUpdatedEvent(
                CourseId: request.CourseId,
                InstructorId: enrollment.InstructorId,
                CourseAvgRating: courseAvg,
                CourseTotalReviews: courseTotal,
                InstructorAvgRating: instructorAvg,
                Timestamp: DateTime.UtcNow
            ));

            logger.LogInformation("Course review created successfully for user {UserId} and course {CourseId}", userId, request.CourseId);
            return ApiResponse<CourseReviewResponse>.SuccessResponse(courseReview.ToResponse(), "Đánh giá khóa học thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating course review for user {UserId}", userId);
            return ApiResponse<CourseReviewResponse>.FailureResponse("Đã xảy ra lỗi khi tạo đánh giá khóa học.");
        }
    }

    public async Task<ApiResponse<List<CourseReviewResponse>>> GetReviewsByCourseIdAsync(GetCourseReviewsRequest request)
    {
        try
        {
            var (items, totalCount) = await unitOfWork.CourseReviewRepository.GetPagedAsync(
                pageNumber: request.PageNumber,
                pageSize: request.PageSize,
                filter: cr => cr.CourseId == request.CourseId && cr.IsPublished,
                orderBy: q => q.OrderByDescending(cr => cr.CreatedAt));

            var list = items.Select(cr => cr.ToResponse()).ToList();
            return ApiResponse<List<CourseReviewResponse>>.SuccessPagedResponse(
                list,
                totalCount,
                request.PageNumber,
                request.PageSize,
                "Lấy danh sách đánh giá thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting reviews for course {CourseId}", request.CourseId);
            return ApiResponse<List<CourseReviewResponse>>.FailureResponse("Đã xảy ra lỗi khi lấy danh sách đánh giá.");
        }
    }

    private async Task<(decimal AvgRating, int TotalReviews)> GetCourseReviewStatsAsync(Guid courseId)
    {
        var reviews = await unitOfWork.CourseReviewRepository.GetAllAsync(cr =>
            cr.CourseId == courseId && cr.IsPublished);
        var count = reviews.Count;
        if (count == 0)
            return (0, 0);
        var avg = (decimal)reviews.Average(r => r.Rating);
        return (Math.Round(avg, 2), count);
    }

    private async Task<decimal> GetInstructorAvgRatingAsync(Guid instructorId)
    {
        var courseIds = await unitOfWork.EnrollmentRepository.AsQueryable()
            .Where(e => e.InstructorId == instructorId && e.DeletedAt == null)
            .Select(e => e.CourseId)
            .Distinct()
            .ToListAsync();
        if (courseIds.Count == 0)
            return 0;
        var reviews = await unitOfWork.CourseReviewRepository.GetAllAsync(cr =>
            courseIds.Contains(cr.CourseId) && cr.IsPublished);
        if (reviews.Count == 0)
            return 0;
        return Math.Round((decimal)reviews.Average(r => r.Rating), 2);
    }
}