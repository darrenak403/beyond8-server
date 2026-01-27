using Beyond8.Catalog.Application.Dtos.Courses;
using Beyond8.Catalog.Application.Mappings.CourseMappings;
using Beyond8.Catalog.Application.Services.Interfaces;
using Beyond8.Catalog.Domain.Entities;
using Beyond8.Catalog.Domain.Enums;
using Beyond8.Catalog.Domain.Repositories.Interfaces;
using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Beyond8.Catalog.Application.Services.Implements;

public class CourseService(
    ILogger<CourseService> logger,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService) : ICourseService
{
    public async Task<ApiResponse<CourseResponse>> CreateCourseAsync(CreateCourseRequest request)
    {
        try
        {
            // Validate category exists
            var category = await unitOfWork.CategoryRepository.FindOneAsync(c => c.Id == request.CategoryId && c.IsActive);
            if (category == null)
            {
                logger.LogWarning("Category not found or inactive: {CategoryId}", request.CategoryId);
                return ApiResponse<CourseResponse>.FailureResponse("Danh mục không tồn tại hoặc không hoạt động.");
            }

            // Check if course title already exists
            var existingCourse = await unitOfWork.CourseRepository.FindOneAsync(c => c.Title == request.Title);
            if (existingCourse != null)
            {
                logger.LogWarning("Course with title already exists: {Title}", request.Title);
                return ApiResponse<CourseResponse>.FailureResponse("Khóa học với tiêu đề này đã tồn tại.");
            }

            var course = request.ToEntity();
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
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

            if (course == null)
            {
                logger.LogWarning("Course not found: {CourseId}", id);
                return ApiResponse<CourseResponse>.FailureResponse("Khóa học không tồn tại.");
            }

            // Authorization check: Only the instructor who owns the course can update it
            if (course.InstructorId != currentUserId)
            {
                logger.LogWarning("Unauthorized update attempt: User {UserId} tried to update course {CourseId} owned by {OwnerId}", currentUserId, id, course.InstructorId);
                return ApiResponse<CourseResponse>.FailureResponse("Bạn không có quyền cập nhật khóa học này.");
            }

            // Status validation: Prevent updates on suspended or archived courses
            if (course.Status == CourseStatus.Suspended || course.Status == CourseStatus.Archived)
            {
                logger.LogWarning("Cannot update course in status {Status}: {CourseId}", course.Status, id);
                return ApiResponse<CourseResponse>.FailureResponse("Không thể cập nhật khóa học ở trạng thái này.");
            }

            // Allow metadata updates for any active status (Draft, PendingApproval, Approved, Published, etc.)
            // Status remains unchanged - no re-approval needed for metadata changes

            // Validate category exists
            var category = await unitOfWork.CategoryRepository.FindOneAsync(c => c.Id == request.CategoryId && c.IsActive);
            if (category == null)
            {
                logger.LogWarning("Category not found or inactive: {CategoryId}", request.CategoryId);
                return ApiResponse<CourseResponse>.FailureResponse("Danh mục không tồn tại hoặc không hoạt động.");
            }

            // Check if new title conflicts with existing courses (excluding current)
            var existingCourse = await unitOfWork.CourseRepository.FindOneAsync(c => c.Title == request.Title && c.Id != id);
            if (existingCourse != null)
            {
                logger.LogWarning("Course with title already exists: {Title}", request.Title);
                return ApiResponse<CourseResponse>.FailureResponse("Khóa học với tiêu đề này đã tồn tại.");
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

    public async Task<ApiResponse<CourseResponse>> UpdateCourseContentAsync(Guid id, Guid currentUserId, UpdateCourseContentRequest request)
    {
        try
        {
            var course = await unitOfWork.CourseRepository
                .AsQueryable()
                .Include(c => c.Category)
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

            if (course == null)
            {
                logger.LogWarning("Course not found: {CourseId}", id);
                return ApiResponse<CourseResponse>.FailureResponse("Khóa học không tồn tại.");
            }

            // Authorization check: Only the instructor who owns the course can update it
            if (course.InstructorId != currentUserId)
            {
                logger.LogWarning("Unauthorized update attempt: User {UserId} tried to update course {CourseId} owned by {OwnerId}", currentUserId, id, course.InstructorId);
                return ApiResponse<CourseResponse>.FailureResponse("Bạn không có quyền cập nhật khóa học này.");
            }

            // Status validation: Prevent updates on suspended or archived courses
            if (course.Status == CourseStatus.Suspended || course.Status == CourseStatus.Archived)
            {
                logger.LogWarning("Cannot update course in status {Status}: {CourseId}", course.Status, id);
                return ApiResponse<CourseResponse>.FailureResponse("Không thể cập nhật khóa học ở trạng thái này.");
            }

            // TODO: Check for active enrollments - if enrollment service is implemented
            // If course has active students enrolled, content updates should be restricted
            // For now, allow but log warning
            // var hasActiveEnrollments = await CheckActiveEnrollmentsAsync(id);
            // if (hasActiveEnrollments && (course.Status == CourseStatus.Published))
            // {
            //     return ApiResponse<CourseResponse>.FailureResponse("Không thể cập nhật nội dung khóa học đang có học viên đang học.");
            // }

            // Content updates require re-approval if course is already approved/published
            if (course.Status == CourseStatus.Approved || course.Status == CourseStatus.Published)
            {
                course.Status = CourseStatus.PendingApproval;
                logger.LogInformation("Course content updated, status changed to PendingApproval: {CourseId}", id);
            }

            course.UpdateContentFromRequest(request);
            await unitOfWork.CourseRepository.UpdateAsync(id, course);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Course content updated successfully: {CourseId}", id);
            return ApiResponse<CourseResponse>.SuccessResponse(course.ToResponse(), "Cập nhật nội dung khóa học thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating course content: {CourseId}", id);
            return ApiResponse<CourseResponse>.FailureResponse("Đã xảy ra lỗi khi cập nhật nội dung khóa học.");
        }
    }

    public async Task<ApiResponse<CourseResponse>> GetCourseByIdAsync(Guid id, Guid currentUserId)
    {
        try
        {
            var course = await unitOfWork.CourseRepository
                .AsQueryable()
                .Include(c => c.Category)
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

            if (course == null)
            {
                logger.LogWarning("Course not found: {CourseId}", id);
                return ApiResponse<CourseResponse>.FailureResponse("Khóa học không tồn tại.");
            }

            // Authorization check: Only the instructor who owns the course can view it
            if (course.InstructorId != currentUserId)
            {
                logger.LogWarning("Unauthorized access attempt: User {UserId} tried to view course {CourseId} owned by {OwnerId}", currentUserId, id, course.InstructorId);
                return ApiResponse<CourseResponse>.FailureResponse("Bạn không có quyền xem khóa học này.");
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
            var course = await unitOfWork.CourseRepository.FindOneAsync(c => c.Id == id);
            if (course == null)
            {
                logger.LogWarning("Course not found: {CourseId}", id);
                return ApiResponse<bool>.FailureResponse("Khóa học không tồn tại.");
            }

            // Authorization check: Only the instructor who owns the course can delete it
            if (course.InstructorId != currentUserId)
            {
                logger.LogWarning("Unauthorized delete attempt: User {UserId} tried to delete course {CourseId} owned by {OwnerId}", currentUserId, id, course.InstructorId);
                return ApiResponse<bool>.FailureResponse("Bạn không có quyền xóa khóa học này.");
            }

            // Status validation: Prevent deletion of published courses with active enrollments
            if (course.Status == CourseStatus.Published)
            {
                // TODO: Check for active enrollments
                logger.LogWarning("Cannot delete published course: {CourseId}", id);
                return ApiResponse<bool>.FailureResponse("Không thể xóa khóa học đã xuất bản.");
            }

            // Check if course has enrollments
            // TODO: Check enrollments when enrollment service is implemented

            // Soft delete - set IsActive flag to false
            course.IsActive = false;
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

    public async Task<ApiResponse<List<CourseResponse>>> GetCoursesByInstructorAsync(Guid instructorId, PaginationRequest pagination)
    {
        try
        {
            var courses = await unitOfWork.CourseRepository.GetPagedAsync(
                pageNumber: pagination.PageNumber,
                pageSize: pagination.PageSize,
                filter: c => c.InstructorId == instructorId && c.IsActive,
                orderBy: query => query.OrderByDescending(c => c.CreatedAt)
            );

            var courseResponses = courses.Items.Select(c => c.ToResponse()).ToList();

            return ApiResponse<List<CourseResponse>>.SuccessPagedResponse(
                courseResponses,
                courses.TotalCount,
                pagination.PageNumber,
                pagination.PageSize,
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
                .FirstOrDefaultAsync(c => c.Id == courseId && c.IsActive);

            if (course == null)
            {
                logger.LogWarning("Course not found: {CourseId}", courseId);
                return ApiResponse<bool>.FailureResponse("Khóa học không tồn tại.");
            }

            // Authorization check: Only the instructor who owns the course can submit for approval
            if (course.InstructorId != currentUserId)
            {
                logger.LogWarning("Unauthorized submit attempt: User {UserId} tried to submit course {CourseId} owned by {OwnerId}", currentUserId, courseId, course.InstructorId);
                return ApiResponse<bool>.FailureResponse("Bạn không có quyền nộp duyệt khóa học này.");
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

    public async Task<ApiResponse<List<CourseResponse>>> GetPendingApprovalCoursesAsync(PaginationRequest pagination)
    {
        try
        {
            var courses = await unitOfWork.CourseRepository.GetPagedAsync(
                pageNumber: pagination.PageNumber,
                pageSize: pagination.PageSize,
                filter: c => c.Status == CourseStatus.PendingApproval && c.IsActive,
                orderBy: query => query.OrderBy(c => c.CreatedAt)
            );

            var courseResponses = courses.Items.Select(c => c.ToResponse()).ToList();

            return ApiResponse<List<CourseResponse>>.SuccessPagedResponse(
                courseResponses,
                courses.TotalCount,
                pagination.PageNumber,
                pagination.PageSize,
                "Lấy danh sách khóa học chờ phê duyệt thành công."
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting pending approval courses");
            return ApiResponse<List<CourseResponse>>.FailureResponse("Đã xảy ra lỗi khi lấy danh sách khóa học chờ phê duyệt.");
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

    public async Task<ApiResponse<bool>> UpdateCourseThumbnailAsync(Guid courseId, Guid currentUserId, string thumbnailUrl)
    {
        try
        {
            var course = await unitOfWork.CourseRepository.FindOneAsync(c => c.Id == courseId && c.IsActive);
            if (course == null)
            {
                logger.LogWarning("Course not found: {CourseId}", courseId);
                return ApiResponse<bool>.FailureResponse("Khóa học không tồn tại.");
            }

            // Authorization check: Only the instructor who owns the course can update thumbnail
            if (course.InstructorId != currentUserId)
            {
                logger.LogWarning("Unauthorized thumbnail update attempt: User {UserId} tried to update course {CourseId} owned by {OwnerId}", currentUserId, courseId, course.InstructorId);
                return ApiResponse<bool>.FailureResponse("Bạn không có quyền cập nhật thumbnail khóa học này.");
            }

            course.ThumbnailUrl = thumbnailUrl;
            await unitOfWork.CourseRepository.UpdateAsync(courseId, course);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Course thumbnail updated: {CourseId}", courseId);
            return ApiResponse<bool>.SuccessResponse(true, "Cập nhật thumbnail thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating course thumbnail: {CourseId}", courseId);
            return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi cập nhật thumbnail.");
        }
    }

    public async Task<ApiResponse<bool>> UpdateCoursePriceAsync(Guid courseId, Guid currentUserId, decimal newPrice)
    {
        try
        {
            var course = await unitOfWork.CourseRepository.FindOneAsync(c => c.Id == courseId && c.IsActive);
            if (course == null)
            {
                logger.LogWarning("Course not found: {CourseId}", courseId);
                return ApiResponse<bool>.FailureResponse("Khóa học không tồn tại.");
            }

            // Authorization check: Only the instructor who owns the course can update price
            if (course.InstructorId != currentUserId)
            {
                logger.LogWarning("Unauthorized price update attempt: User {UserId} tried to update course {CourseId} owned by {OwnerId}", currentUserId, courseId, course.InstructorId);
                return ApiResponse<bool>.FailureResponse("Bạn không có quyền cập nhật giá khóa học này.");
            }

            if (newPrice < 0 || newPrice > 100000000)
            {
                return ApiResponse<bool>.FailureResponse("Giá khóa học phải từ 0 đến 100 triệu VND.");
            }

            course.Price = newPrice;
            await unitOfWork.CourseRepository.UpdateAsync(courseId, course);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Course price updated: {CourseId}, new price: {Price}", courseId, newPrice);
            return ApiResponse<bool>.SuccessResponse(true, "Cập nhật giá khóa học thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating course price: {CourseId}", courseId);
            return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi cập nhật giá khóa học.");
        }
    }
}