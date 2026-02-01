using Beyond8.Catalog.Application.Clients.Identity;
using Beyond8.Catalog.Application.Dtos.Courses;
using Beyond8.Catalog.Application.Dtos.Users;
using Beyond8.Catalog.Application.Mappings.CourseMappings;
using Beyond8.Catalog.Application.Services.Interfaces;
using Beyond8.Catalog.Domain.Entities;
using Beyond8.Catalog.Domain.Enums;
using Beyond8.Catalog.Domain.Repositories.Interfaces;
using Beyond8.Common.Events.Catalog;
using Beyond8.Common.Utilities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Beyond8.Catalog.Application.Services.Implements;

public class CourseService(
    ILogger<CourseService> logger,
    IUnitOfWork unitOfWork,
    IIdentityClient identityClient,
    IPublishEndpoint publishEndpoint) : ICourseService
{

    private static (int PageNumber, int PageSize) NormalizePagination(PaginationCourseSearchRequest request)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize switch
        {
            < 1 => 10,
            > 100 => 100,
            _ => request.PageSize
        };
        return (pageNumber, pageSize);
    }

    private async Task<(bool IsValid, string? ErrorMessage)> ValidateCategoryExistsAsync(Guid? categoryId)
    {
        if (!categoryId.HasValue)
            return (true, null);

        var category = await unitOfWork.CategoryRepository.FindOneAsync(c => c.Id == categoryId && c.IsActive && !c.IsRoot);
        if (category == null)
        {
            logger.LogWarning("Category not found or inactive: {CategoryId}", categoryId);
            return (false, "Danh mục không tồn tại hoặc không hoạt động.");
        }
        return (true, null);
    }

    private async Task<(bool IsValid, Course? Course, string? ErrorMessage)> CheckCourseOwnershipAsync(Guid courseId, Guid currentUserId)
    {
        var course = await unitOfWork.CourseRepository
            .AsQueryable()
            .Include(c => c.Category)
            .FirstOrDefaultAsync(c => c.Id == courseId && c.IsActive && c.InstructorId == currentUserId);

        if (course == null)
        {
            logger.LogWarning("Course not found: {CourseId}", courseId);
            return (false, null, "Khóa học không tồn tại.");
        }
        return (true, course, null);
    }

    private async Task<(bool IsValid, Course? Course, string? ErrorMessage)> GetCourseWithSectionsForInstructorAsync(Guid courseId, Guid currentUserId)
    {
        var course = await unitOfWork.CourseRepository
            .AsQueryable()
            .Include(c => c.Sections)
            .ThenInclude(s => s.Lessons)
            .FirstOrDefaultAsync(c => c.Id == courseId && c.IsActive && c.InstructorId == currentUserId);

        if (course == null)
        {
            logger.LogWarning("Course not found: {CourseId}", courseId);
            return (false, null, "Khóa học không tồn tại.");
        }
        return (true, course, null);
    }

    private static decimal GetAverageRatingSafe(List<Course> courses)
    {
        var withRating = courses.Where(c => c.AvgRating.HasValue).ToList();
        return withRating.Count > 0 ? withRating.Average(c => c.AvgRating!.Value) : 0;
    }

    public async Task<ApiResponse<List<CourseResponse>>> GetAllCoursesAsync(PaginationCourseSearchRequest request)
    {
        try
        {
            var (pageNumber, pageSize) = NormalizePagination(request);

            var (courses, totalCount) = await unitOfWork.CourseRepository.SearchCoursesAsync(
                pageNumber,
                pageSize,
                keyword: request.Keyword,
                categoryName: request.CategoryName,
                instructorName: request.InstructorName,
                status: request.Status,
                level: request.Level,
                language: request.Language,
                minPrice: request.MinPrice,
                maxPrice: request.MaxPrice,
                minRating: request.MinRating,
                minStudents: request.MinStudents,
                isActive: request.IsActive,
                isDescending: request.IsDescending,
                isDescendingPrice: request.IsDescendingPrice,
                isRandom: request.IsRandom
            );

            var courseResponses = courses.Select(c => c.ToResponse()).ToList();

            return ApiResponse<List<CourseResponse>>.SuccessPagedResponse(
                courseResponses,
                totalCount,
                pageNumber,
                pageSize,
                "Lấy danh sách khóa học thành công."
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting all courses");
            return ApiResponse<List<CourseResponse>>.FailureResponse("Đã xảy ra lỗi khi lấy danh sách khóa học.");
        }
    }

    public async Task<ApiResponse<CourseResponse>> CreateCourseAsync(CreateCourseRequest request, Guid currentUserId)
    {
        try
        {
            var categoryValidation = await ValidateCategoryExistsAsync(request.CategoryId);
            if (!categoryValidation.IsValid)
                return ApiResponse<CourseResponse>.FailureResponse(categoryValidation.ErrorMessage!);

            // Get instructor name
            var instructorResponse = await GetInstructorInfoAsync(currentUserId);
            if (!instructorResponse.IsValid)
                return ApiResponse<CourseResponse>.FailureResponse(instructorResponse.ErrorMessage!);

            var course = request.ToEntity(currentUserId, instructorResponse.User!.FullName);
            await unitOfWork.CourseRepository.AddAsync(course);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Course created successfully: {CourseId} by instructor: {InstructorId}", course.Id, course.InstructorId);
            return ApiResponse<CourseResponse>.SuccessResponse(course.ToResponse(), "Tạo khóa học thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating course with title: {Title}", request.Title);
            return ApiResponse<CourseResponse>.FailureResponse("Đã xảy ra lỗi khi tạo khóa học.");
        }
    }

    public async Task<ApiResponse<CourseResponse>> UpdateCourseMetadataAsync(Guid id, Guid currentUserId, UpdateCourseMetadataRequest request)
    {
        try
        {
            var (isValid, course, errorMessage) = await CheckCourseOwnershipAsync(id, currentUserId);
            if (!isValid)
                return ApiResponse<CourseResponse>.FailureResponse(errorMessage!);

            // Status validation: Prevent updates on suspended or archived courses
            if (course!.Status == CourseStatus.Suspended || course.Status == CourseStatus.Archived)
            {
                logger.LogWarning("Cannot update course in status {Status}: {CourseId}", course.Status, id);
                return ApiResponse<CourseResponse>.FailureResponse("Không thể cập nhật khóa học ở trạng thái này.");
            }

            var categoryValidation = await ValidateCategoryExistsAsync(request.CategoryId);
            if (!categoryValidation.IsValid)
                return ApiResponse<CourseResponse>.FailureResponse(categoryValidation.ErrorMessage!);

            course.UpdateMetadataFromRequest(request);
            await unitOfWork.CourseRepository.UpdateAsync(id, course);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Course metadata updated successfully: {CourseId}", id);
            return ApiResponse<CourseResponse>.SuccessResponse(course.ToResponse(), "Cập nhật thông tin khóa học thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating course metadata: {CourseId}", id);
            return ApiResponse<CourseResponse>.FailureResponse("Đã xảy ra lỗi khi cập nhật thông tin khóa học.");
        }
    }
    public async Task<ApiResponse<CourseResponse>> GetCourseByIdAsync(Guid id, Guid currentUserId)
    {
        try
        {
            var (isValid, course, errorMessage) = await CheckCourseOwnershipAsync(id, currentUserId);
            if (!isValid)
                return ApiResponse<CourseResponse>.FailureResponse(errorMessage!);

            return ApiResponse<CourseResponse>.SuccessResponse(course!.ToResponse(), "Lấy thông tin khóa học thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting course by id: {CourseId}", id);
            return ApiResponse<CourseResponse>.FailureResponse("Đã xảy ra lỗi khi lấy thông tin khóa học.");
        }
    }

    public async Task<ApiResponse<bool>> DeleteCourseAsync(Guid id, Guid currentUserId)
    {
        try
        {
            var (isValid, course, errorMessage) = await CheckCourseOwnershipAsync(id, currentUserId);
            if (!isValid)
                return ApiResponse<bool>.FailureResponse(errorMessage!);

            // Soft delete (IsActive=false). Cấm xóa khóa đã xuất bản. Section/Lesson vẫn tồn tại.
            if (course!.Status == CourseStatus.Published)
            {
                logger.LogWarning("Cannot delete published course: {CourseId}", id);
                return ApiResponse<bool>.FailureResponse("Không thể xóa khóa học đã xuất bản.");
            }

            course!.IsActive = false;
            course.DeletedAt = DateTime.UtcNow;
            course.DeletedBy = currentUserId;
            await unitOfWork.CourseRepository.UpdateAsync(id, course);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Course deleted successfully: {CourseId}", id);
            return ApiResponse<bool>.SuccessResponse(true, "Xóa khóa học thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting course: {CourseId}", id);
            return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi xóa khóa học.");
        }
    }

    public async Task<ApiResponse<List<CourseResponse>>> GetCoursesByInstructorAsync(Guid instructorId, PaginationCourseSearchRequest request)
    {
        try
        {
            logger.LogInformation("Getting courses for instructor: {InstructorId}", instructorId);

            var (pageNumber, pageSize) = NormalizePagination(request);

            var (courses, totalCount) = await unitOfWork.CourseRepository.SearchCoursesInstructorAsync(
                pageNumber,
                pageSize,
                keyword: request.Keyword,
                categoryName: request.CategoryName,
                instructorName: request.InstructorName,
                status: request.Status,
                level: request.Level,
                language: request.Language,
                minPrice: request.MinPrice,
                maxPrice: request.MaxPrice,
                minRating: request.MinRating,
                minStudents: request.MinStudents,
                isActive: request.IsActive,
                isDescendingPrice: request.IsDescendingPrice,
                isDescending: request.IsDescending,
                isRandom: request.IsRandom,
                instructorId: instructorId
            );

            var courseResponses = courses.Select(c => c.ToResponse()).ToList();

            return ApiResponse<List<CourseResponse>>.SuccessPagedResponse(
                courseResponses,
                totalCount,
                pageNumber,
                pageSize,
                "Lấy danh sách khóa học thành công."
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting courses by instructor: {InstructorId}", instructorId);
            return ApiResponse<List<CourseResponse>>.FailureResponse("Đã xảy ra lỗi khi lấy danh sách khóa học.");
        }
    }

    public async Task<ApiResponse<CourseStatsResponse>> GetCourseStatsByInstructorAsync(Guid instructorId)
    {
        try
        {
            var courses = await unitOfWork.CourseRepository
                .AsQueryable()
                .Where(c => c.InstructorId == instructorId && c.IsActive)
                .ToListAsync();

            var stats = new CourseStatsResponse
            {
                TotalCourses = courses.Count,
                DraftCourses = courses.Count(c => c.Status == CourseStatus.Draft),
                PendingApprovalCourses = courses.Count(c => c.Status == CourseStatus.PendingApproval),
                PublishedCourses = courses.Count(c => c.Status == CourseStatus.Published),
                RejectedCourses = courses.Count(c => c.Status == CourseStatus.Rejected),
                // TODO(event): TotalStudents, TotalRevenue - khi có Enrollment service: consume event hoặc gọi client
                TotalStudents = 0,
                TotalRevenue = 0,
                AverageRating = GetAverageRatingSafe(courses),
                TotalReviews = courses.Sum(c => c.TotalReviews)
            };

            return ApiResponse<CourseStatsResponse>.SuccessResponse(stats, "Lấy thống kê khóa học thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting course stats for instructor: {InstructorId}", instructorId);
            return ApiResponse<CourseStatsResponse>.FailureResponse("Đã xảy ra lỗi khi lấy thống kê khóa học.");
        }
    }

    public async Task<ApiResponse<bool>> SubmitForApprovalAsync(Guid courseId, Guid currentUserId)
    {
        try
        {
            var (isValid, course, errorMessage) = await GetCourseWithSectionsForInstructorAsync(courseId, currentUserId);
            if (!isValid)
                return ApiResponse<bool>.FailureResponse(errorMessage!);

            // Validate course can be submitted
            if (course!.Status != CourseStatus.Draft)
            {
                return ApiResponse<bool>.FailureResponse("Chỉ có thể nộp duyệt khóa học ở trạng thái bản nháp.");
            }

            // Validate minimum requirements
            if (!course.Sections.Any())
            {
                return ApiResponse<bool>.FailureResponse("Khóa học phải có ít nhất 1 chương.");
            }

            var totalLessons = course.Sections.Sum(s => s.Lessons.Count);
            if (totalLessons < 3)
            {
                return ApiResponse<bool>.FailureResponse("Khóa học phải có ít nhất 3 bài học.");
            }

            var isAllVideosUploaded = await ValidateAllVideosUploadedAsync(course);
            if (!isAllVideosUploaded)
            {
                return ApiResponse<bool>.FailureResponse("Tất cả video phải được upload và transcoded (HLS) trước khi cho submit.");
            }

            course.Status = CourseStatus.PendingApproval;
            await unitOfWork.CourseRepository.UpdateAsync(courseId, course);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Course submitted for approval: {CourseId}", courseId);
            return ApiResponse<bool>.SuccessResponse(true, "Nộp duyệt khóa học thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error submitting course for approval: {CourseId}", courseId);
            return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi nộp duyệt khóa học.");
        }
    }

    private async Task<bool> ValidateAllVideosUploadedAsync(Course course)
    {
        var lesson = await unitOfWork.LessonRepository.AsQueryable()
            .Include(l => l.Video)
            .Include(l => l.Section)
            .ThenInclude(s => s.Course)
            .Where(l => l.Section.CourseId == course.Id && l.Video != null)
            .ToListAsync();
        return lesson.All(l => !string.IsNullOrEmpty(l.Video!.VideoOriginalUrl) && !string.IsNullOrEmpty(l.Video!.HlsVariants));
    }

    public async Task<ApiResponse<bool>> ApproveCourseAsync(Guid courseId, ApproveCourseRequest request)
    {
        try
        {
            var course = await unitOfWork.CourseRepository.FindOneAsync(c => c.Id == courseId && c.IsActive);
            if (course == null)
            {
                logger.LogWarning("Course not found: {CourseId}", courseId);
                return ApiResponse<bool>.FailureResponse("Khóa học không tồn tại.");
            }

            if (course.Status != CourseStatus.PendingApproval)
            {
                return ApiResponse<bool>.FailureResponse("Khóa học không ở trạng thái chờ phê duyệt.");
            }

            course.Status = CourseStatus.Approved;
            course.ApprovalNotes = request.Notes;
            await unitOfWork.CourseRepository.UpdateAsync(courseId, course);
            await unitOfWork.SaveChangesAsync();

            var instructorInfo = await GetInstructorInfoAsync(course.InstructorId);
            if (!instructorInfo.IsValid)
                return ApiResponse<bool>.FailureResponse(instructorInfo.ErrorMessage!);

            // Publish event to integration service (email notification)
            await publishEndpoint.Publish(new CourseApprovedEvent(
                courseId,
                course.InstructorId,
                instructorInfo.User!.Email,
                instructorInfo.User!.FullName,
                course.Title,
                request.Notes,
                DateTime.UtcNow));

            logger.LogInformation("Course approved: {CourseId}", courseId);
            return ApiResponse<bool>.SuccessResponse(true, "Phê duyệt khóa học thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error approving course: {CourseId}", courseId);
            return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi phê duyệt khóa học.");
        }
    }

    public async Task<ApiResponse<bool>> RejectCourseAsync(Guid courseId, RejectCourseRequest request)
    {
        try
        {
            var course = await unitOfWork.CourseRepository.FindOneAsync(c => c.Id == courseId && c.IsActive);
            if (course == null)
            {
                logger.LogWarning("Course not found: {CourseId}", courseId);
                return ApiResponse<bool>.FailureResponse("Khóa học không tồn tại.");
            }

            if (course.Status != CourseStatus.PendingApproval)
            {
                return ApiResponse<bool>.FailureResponse("Khóa học không ở trạng thái chờ phê duyệt.");
            }

            course.Status = CourseStatus.Rejected;
            course.RejectionReason = request.Reason;
            await unitOfWork.CourseRepository.UpdateAsync(courseId, course);
            await unitOfWork.SaveChangesAsync();

            var instructorInfor = await GetInstructorInfoAsync(course.InstructorId);
            if (!instructorInfor.IsValid)
                return ApiResponse<bool>.FailureResponse(instructorInfor.ErrorMessage!);

            // Publish event to notification service
            await publishEndpoint.Publish(new CourseRejectedEvent(courseId, course.InstructorId, instructorInfor.User!.Email, instructorInfor.User!.FullName, course.Title, request.Reason, DateTime.UtcNow));

            logger.LogInformation("Course rejected: {CourseId}", courseId);
            return ApiResponse<bool>.SuccessResponse(true, "Từ chối khóa học thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error rejecting course: {CourseId}", courseId);
            return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi từ chối khóa học.");
        }
    }

    public async Task<ApiResponse<List<CourseResponse>>> GetAllCoursesForAdminAsync(PaginationCourseSearchRequest request)
    {
        try
        {
            var (pageNumber, pageSize) = NormalizePagination(request);

            var (courses, totalCount) = await unitOfWork.CourseRepository.SearchCoursesAdminAsync(
                pageNumber,
                pageSize,
                keyword: request.Keyword,
                categoryName: request.CategoryName,
                instructorName: request.InstructorName,
                status: request.Status,
                level: request.Level,
                language: request.Language,
                minPrice: request.MinPrice,
                maxPrice: request.MaxPrice,
                minRating: request.MinRating,
                minStudents: request.MinStudents,
                isActive: request.IsActive,
                isDescendingPrice: request.IsDescendingPrice,
                isDescending: request.IsDescending,
                isRandom: request.IsRandom
            );

            var courseResponses = courses.Select(c => c.ToResponse()).ToList();

            return ApiResponse<List<CourseResponse>>.SuccessPagedResponse(
                courseResponses,
                totalCount,
                pageNumber,
                pageSize,
                "Lấy danh sách khóa học thành công."
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting courses for admin");
            return ApiResponse<List<CourseResponse>>.FailureResponse("Đã xảy ra lỗi khi lấy danh sách khóa học.");
        }
    }

    public async Task<ApiResponse<bool>> PublishCourseAsync(Guid courseId, Guid currentUserId)
    {
        try
        {
            var (isValid, course, errorMessage) = await CheckCourseOwnershipAsync(courseId, currentUserId);
            if (!isValid)
                return ApiResponse<bool>.FailureResponse(errorMessage!);

            if (course!.Status != CourseStatus.Approved)
            {
                return ApiResponse<bool>.FailureResponse("Chỉ có thể công bố khóa học đã được phê duyệt.");
            }

            course.Status = CourseStatus.Published;
            await unitOfWork.CourseRepository.UpdateAsync(courseId, course);
            await unitOfWork.SaveChangesAsync();

            var instructorInfor = await identityClient.GetUserByIdAsync(course.InstructorId);
            if (!instructorInfor.IsSuccess || instructorInfor.Data == null)
            {
                logger.LogWarning("Failed to get instructor info for user: {UserId}", course.InstructorId);
                return ApiResponse<bool>.FailureResponse("Không thể lấy thông tin giảng viên.");
            }

            // Publish event to integration service
            var courseUrl = $"https://beyond8.dev/courses/{course.Slug}";
            await publishEndpoint.Publish(new CoursePublishedEvent(courseId, course.InstructorId, instructorInfor.Data.Email, instructorInfor.Data.FullName, course.Title, courseUrl, DateTime.UtcNow));

            logger.LogInformation("Course published: {CourseId}", courseId);
            return ApiResponse<bool>.SuccessResponse(true, "Công bố khóa học thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing course: {CourseId}", courseId);
            return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi công bố khóa học.");
        }
    }

    public async Task<ApiResponse<bool>> UnpublishCourseAsync(Guid courseId, Guid currentUserId)
    {
        try
        {
            var (isValid, course, errorMessage) = await CheckCourseOwnershipAsync(courseId, currentUserId);
            if (!isValid)
                return ApiResponse<bool>.FailureResponse(errorMessage!);

            if (course!.Status != CourseStatus.Published)
            {
                return ApiResponse<bool>.FailureResponse("Khóa học không ở trạng thái công khai.");
            }

            course.Status = CourseStatus.Approved;
            await unitOfWork.CourseRepository.UpdateAsync(courseId, course);
            await unitOfWork.SaveChangesAsync();

            // Publish event to identity service
            await publishEndpoint.Publish(new CourseUnpublishedEvent(courseId, course.InstructorId, DateTime.UtcNow));

            logger.LogInformation("Course unpublished: {CourseId}", courseId);
            return ApiResponse<bool>.SuccessResponse(true, "Ẩn khóa học thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error unpublishing course: {CourseId}", courseId);
            return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi ẩn khóa học.");
        }
    }

    public async Task<ApiResponse<bool>> UpdateCourseThumbnailAsync(Guid courseId, Guid currentUserId, UpdateCourseThumbnailRequest request)
    {
        try
        {
            var (isValid, course, errorMessage) = await CheckCourseOwnershipAsync(courseId, currentUserId);
            if (!isValid)
                return ApiResponse<bool>.FailureResponse(errorMessage!);

            course!.ThumbnailUrl = request.ThumbnailUrl;
            await unitOfWork.CourseRepository.UpdateAsync(courseId, course);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Course thumbnail updated successfully: {CourseId}", courseId);
            return ApiResponse<bool>.SuccessResponse(true, "Cập nhật ảnh đại diện khóa học thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating course thumbnail: {CourseId}", courseId);
            return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi cập nhật ảnh đại diện khóa học.");
        }
    }

    private async Task<(bool IsValid, UserSimpleResponse? User, string? ErrorMessage)> GetInstructorInfoAsync(Guid instructorId)
    {
        var instructorInfor = await identityClient.GetUserByIdAsync(instructorId);
        if (!instructorInfor.IsSuccess || instructorInfor.Data == null)
        {
            logger.LogWarning("Failed to get instructor info for user: {UserId}", instructorId);
            return (false, null, "Không thể lấy thông tin giảng viên.");
        }
        return (true, instructorInfor.Data, null);
    }

    public async Task<ApiResponse<CourseSummaryResponse>> GetCourseSummaryAsync(Guid courseId)
    {
        try
        {
            var course = await unitOfWork.CourseRepository
                .AsQueryable()
                .Include(c => c.Category)
                .Include(c => c.Sections.Where(s => s.IsPublished))
                    .ThenInclude(s => s.Lessons.Where(l => l.IsPublished))
                        .ThenInclude(l => l.Video)
                .Include(c => c.Sections.Where(s => s.IsPublished))
                    .ThenInclude(s => s.Lessons.Where(l => l.IsPublished))
                        .ThenInclude(l => l.Text)
                .Include(c => c.Sections.Where(s => s.IsPublished))
                    .ThenInclude(s => s.Lessons.Where(l => l.IsPublished))
                        .ThenInclude(l => l.Quiz)
                .FirstOrDefaultAsync(c => c.Id == courseId && c.IsActive && c.Status == CourseStatus.Published);

            if (course == null)
            {
                logger.LogWarning("Course not found or not published: {CourseId}", courseId);
                return ApiResponse<CourseSummaryResponse>.FailureResponse("Khóa học không tồn tại hoặc chưa được xuất bản.");
            }

            return ApiResponse<CourseSummaryResponse>.SuccessResponse(
                course.ToSummaryResponse(),
                "Lấy tóm tắt khóa học thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting course summary: {CourseId}", courseId);
            return ApiResponse<CourseSummaryResponse>.FailureResponse("Đã xảy ra lỗi khi lấy tóm tắt khóa học.");
        }
    }

    public async Task<ApiResponse<CourseDetailResponse>> GetCourseDetailsAsync(Guid courseId, Guid userId)
    {
        try
        {
            var course = await unitOfWork.CourseRepository
                .AsQueryable()
                .Include(c => c.Category)
                .Include(c => c.Sections.Where(s => s.IsPublished))
                    .ThenInclude(s => s.Lessons.Where(l => l.IsPublished))
                        .ThenInclude(l => l.Video)
                .Include(c => c.Sections.Where(s => s.IsPublished))
                    .ThenInclude(s => s.Lessons.Where(l => l.IsPublished))
                        .ThenInclude(l => l.Text)
                .Include(c => c.Sections.Where(s => s.IsPublished))
                    .ThenInclude(s => s.Lessons.Where(l => l.IsPublished))
                        .ThenInclude(l => l.Quiz)
                .FirstOrDefaultAsync(c => c.Id == courseId && c.IsActive && c.Status == CourseStatus.Published);

            if (course == null)
            {
                logger.LogWarning("Course not found or not published: {CourseId}", courseId);
                return ApiResponse<CourseDetailResponse>.FailureResponse("Khóa học không tồn tại hoặc chưa được xuất bản.");
            }

            // TODO: Check enrollment when Enrollment service is ready
            // var isEnrolled = await enrollmentClient.CheckEnrollmentAsync(userId, courseId);
            // if (!isEnrolled)
            // {
            //     return ApiResponse<CourseDetailResponse>.FailureResponse("Bạn chưa đăng ký khóa học này.");
            // }

            logger.LogInformation("User {UserId} accessed course details: {CourseId}", userId, courseId);

            return ApiResponse<CourseDetailResponse>.SuccessResponse(
                course.ToDetailResponse(),
                "Lấy chi tiết khóa học thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting course details: {CourseId}", courseId);
            return ApiResponse<CourseDetailResponse>.FailureResponse("Đã xảy ra lỗi khi lấy chi tiết khóa học.");
        }
    }
}