using Beyond8.Catalog.Application.Clients.Identity;
using Beyond8.Catalog.Application.Dtos.Courses;
using Beyond8.Catalog.Application.Mappings.CourseMappings;
using Beyond8.Catalog.Application.Services.Interfaces;
using Beyond8.Catalog.Domain.Enums;
using Beyond8.Catalog.Domain.Repositories.Interfaces;
using Beyond8.Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Beyond8.Catalog.Application.Services.Implements;

public class CourseService(
    ILogger<CourseService> logger,
    IUnitOfWork unitOfWork,
    IIdentityClient identityClient) : ICourseService
{

    public async Task<ApiResponse<List<CourseResponse>>> GetAllCoursesAsync(PaginationCourseSearchRequest request)
    {
        try
        {
            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize switch
            {
                < 1 => 10,
                > 100 => 100,
                _ => request.PageSize
            };

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
            // Validate category exists if provided
            if (request.CategoryId.HasValue)
            {
                var category = await unitOfWork.CategoryRepository.FindOneAsync(c => c.Id == request.CategoryId && c.IsActive && !c.IsRoot);
                if (category == null)
                {
                    logger.LogWarning("Category not found or inactive: {CategoryId}", request.CategoryId);
                    return ApiResponse<CourseResponse>.FailureResponse("Danh mục không tồn tại hoặc không hoạt động.");
                }
            }

            // Get instructor name
            var instructorResponse = await identityClient.GetUserByIdAsync(currentUserId);
            if (!instructorResponse.IsSuccess || instructorResponse.Data == null)
            {
                logger.LogWarning("Failed to get instructor info for user: {UserId}", currentUserId);
                return ApiResponse<CourseResponse>.FailureResponse("Không thể lấy thông tin giảng viên.");
            }

            var course = request.ToEntity(currentUserId, instructorResponse.Data.FullName);
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
            var course = await unitOfWork.CourseRepository
                .AsQueryable()
                .Include(c => c.Category)
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive && c.InstructorId == currentUserId);

            if (course == null)
            {
                logger.LogWarning("Course not found: {CourseId}", id);
                return ApiResponse<CourseResponse>.FailureResponse("Khóa học không tồn tại.");
            }

            // Status validation: Prevent updates on suspended or archived courses
            if (course.Status == CourseStatus.Suspended || course.Status == CourseStatus.Archived)
            {
                logger.LogWarning("Cannot update course in status {Status}: {CourseId}", course.Status, id);
                return ApiResponse<CourseResponse>.FailureResponse("Không thể cập nhật khóa học ở trạng thái này.");
            }

            // Validate category exists if provided
            if (request.CategoryId.HasValue)
            {
                var category = await unitOfWork.CategoryRepository.FindOneAsync(c => c.Id == request.CategoryId && c.IsActive && !c.IsRoot);
                if (category == null)
                {
                    logger.LogWarning("Category not found or inactive: {CategoryId}", request.CategoryId);
                    return ApiResponse<CourseResponse>.FailureResponse("Danh mục không tồn tại hoặc không hoạt động.");
                }
            }

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
            var course = await unitOfWork.CourseRepository
                .AsQueryable()
                .Include(c => c.Category)
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive && c.InstructorId == currentUserId);

            if (course == null)
            {
                logger.LogWarning("Course not found: {CourseId}", id);
                return ApiResponse<CourseResponse>.FailureResponse("Khóa học không tồn tại.");
            }

            return ApiResponse<CourseResponse>.SuccessResponse(course.ToResponse(), "Lấy thông tin khóa học thành công.");
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
            var course = await unitOfWork.CourseRepository.FindOneAsync(c => c.Id == id && c.IsActive && c.InstructorId == currentUserId);
            if (course == null)
            {
                logger.LogWarning("Course not found: {CourseId}", id);
                return ApiResponse<bool>.FailureResponse("Khóa học không tồn tại.");
            }

            // Soft delete (IsActive=false). Cấm xóa khóa đã xuất bản. Section/Lesson vẫn tồn tại.
            if (course.Status == CourseStatus.Published)
            {
                logger.LogWarning("Cannot delete published course: {CourseId}", id);
                return ApiResponse<bool>.FailureResponse("Không thể xóa khóa học đã xuất bản.");
            }

            course.IsActive = false;
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

            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize switch
            {
                < 1 => 10,
                > 100 => 100,
                _ => request.PageSize
            };

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

    public async Task<ApiResponse<CourseStatsDto>> GetCourseStatsByInstructorAsync(Guid instructorId)
    {
        try
        {
            var courses = await unitOfWork.CourseRepository
                .AsQueryable()
                .Where(c => c.InstructorId == instructorId && c.IsActive)
                .ToListAsync();

            var stats = new CourseStatsDto
            {
                TotalCourses = courses.Count,
                DraftCourses = courses.Count(c => c.Status == CourseStatus.Draft),
                PendingApprovalCourses = courses.Count(c => c.Status == CourseStatus.PendingApproval),
                PublishedCourses = courses.Count(c => c.Status == CourseStatus.Published),
                RejectedCourses = courses.Count(c => c.Status == CourseStatus.Rejected),
                // TODO: Calculate from enrollments when enrollment service is implemented
                TotalStudents = 0,
                TotalRevenue = 0,
                AverageRating = courses.Where(c => c.AvgRating.HasValue).Average(c => c.AvgRating) ?? 0,
                TotalReviews = courses.Sum(c => c.TotalReviews)
            };

            return ApiResponse<CourseStatsDto>.SuccessResponse(stats, "Lấy thống kê khóa học thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting course stats for instructor: {InstructorId}", instructorId);
            return ApiResponse<CourseStatsDto>.FailureResponse("Đã xảy ra lỗi khi lấy thống kê khóa học.");
        }
    }

    public async Task<ApiResponse<bool>> SubmitForApprovalAsync(Guid courseId, Guid currentUserId)
    {
        try
        {
            var course = await unitOfWork.CourseRepository
                .AsQueryable()
                .Include(c => c.Sections)
                .ThenInclude(s => s.Lessons)
                .FirstOrDefaultAsync(c => c.Id == courseId && c.IsActive && c.InstructorId == currentUserId);

            if (course == null)
            {
                logger.LogWarning("Course not found: {CourseId}", courseId);
                return ApiResponse<bool>.FailureResponse("Khóa học không tồn tại.");
            }

            // Validate course can be submitted
            if (course.Status != CourseStatus.Draft)
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

            // TODO: Check if all videos are uploaded and transcoded

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
            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize switch
            {
                < 1 => 10,
                > 100 => 100,
                _ => request.PageSize
            };

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
            var course = await unitOfWork.CourseRepository.FindOneAsync(c => c.Id == courseId && c.IsActive);
            if (course == null)
            {
                logger.LogWarning("Course not found: {CourseId}", courseId);
                return ApiResponse<bool>.FailureResponse("Khóa học không tồn tại.");
            }

            // Authorization check: Only the instructor who owns the course can publish it
            if (course.InstructorId != currentUserId)
            {
                logger.LogWarning("Unauthorized publish attempt: User {UserId} tried to publish course {CourseId} owned by {OwnerId}", currentUserId, courseId, course.InstructorId);
                return ApiResponse<bool>.FailureResponse("Bạn không có quyền công bố khóa học này.");
            }

            if (course.Status != CourseStatus.Approved)
            {
                return ApiResponse<bool>.FailureResponse("Chỉ có thể công bố khóa học đã được phê duyệt.");
            }

            course.Status = CourseStatus.Published;
            await unitOfWork.CourseRepository.UpdateAsync(courseId, course);
            await unitOfWork.SaveChangesAsync();

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
            var course = await unitOfWork.CourseRepository.FindOneAsync(c => c.Id == courseId && c.IsActive);
            if (course == null)
            {
                logger.LogWarning("Course not found: {CourseId}", courseId);
                return ApiResponse<bool>.FailureResponse("Khóa học không tồn tại.");
            }

            // Authorization check: Only the instructor who owns the course can unpublish it
            if (course.InstructorId != currentUserId)
            {
                logger.LogWarning("Unauthorized unpublish attempt: User {UserId} tried to unpublish course {CourseId} owned by {OwnerId}", currentUserId, courseId, course.InstructorId);
                return ApiResponse<bool>.FailureResponse("Bạn không có quyền gỡ bỏ công bố khóa học này.");
            }

            if (course.Status != CourseStatus.Published)
            {
                return ApiResponse<bool>.FailureResponse("Khóa học không ở trạng thái công khai.");
            }

            course.Status = CourseStatus.Approved;
            await unitOfWork.CourseRepository.UpdateAsync(courseId, course);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Course unpublished: {CourseId}", courseId);
            return ApiResponse<bool>.SuccessResponse(true, "Ẩn khóa học thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error unpublishing course: {CourseId}", courseId);
            return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi ẩn khóa học.");
        }
    }
}