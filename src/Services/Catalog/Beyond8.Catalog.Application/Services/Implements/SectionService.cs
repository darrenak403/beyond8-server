using Beyond8.Catalog.Application.Dtos.Sections;
using Beyond8.Catalog.Application.Mappings.SectionMappings;
using Beyond8.Catalog.Application.Services.Interfaces;
using Beyond8.Catalog.Domain.Entities;
using Beyond8.Catalog.Domain.Enums;
using Beyond8.Catalog.Domain.Repositories.Interfaces;
using Beyond8.Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Beyond8.Catalog.Application.Services.Implements;

public class SectionService(
    ILogger<SectionService> logger,
    IUnitOfWork unitOfWork) : ISectionService
{
    public async Task<ApiResponse<List<SectionResponse>>> GetSectionsByCourseIdAsync(Guid courseId, Guid currentUserId)
    {
        try
        {
            var validation = await CheckCourseOwnershipAsync(courseId, currentUserId);
            if (!validation.IsValid)
                return ApiResponse<List<SectionResponse>>.FailureResponse(validation.ErrorMessage!);

            var sections = await unitOfWork.SectionRepository.AsQueryable()
                .Where(s => s.CourseId == courseId)
                .OrderBy(s => s.OrderIndex)
                .ToListAsync();

            var responses = sections.Select(s => s.ToResponse()).ToList();

            return ApiResponse<List<SectionResponse>>.SuccessResponse(responses, "Lấy danh sách chương thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting sections for course: {CourseId}", courseId);
            return ApiResponse<List<SectionResponse>>.FailureResponse("Đã xảy ra lỗi khi lấy danh sách chương.");
        }
    }

    public async Task<ApiResponse<SectionResponse>> GetSectionByIdAsync(Guid sectionId, Guid currentUserId)
    {
        try
        {
            var (isValid, section, errorMessage) = await CheckSectionOwnershipAsync(sectionId, currentUserId);
            if (!isValid)
                return ApiResponse<SectionResponse>.FailureResponse(errorMessage!);

            return ApiResponse<SectionResponse>.SuccessResponse(section!.ToResponse(), "Lấy thông tin chương thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting section by id: {SectionId}", sectionId);
            return ApiResponse<SectionResponse>.FailureResponse("Đã xảy ra lỗi khi lấy thông tin chương.");
        }
    }

    public async Task<ApiResponse<SectionResponse>> CreateSectionAsync(CreateSectionRequest request, Guid currentUserId)
    {
        try
        {
            var validation = await CheckCourseOwnershipAsync(request.CourseId, currentUserId, isPublished: true);
            if (!validation.IsValid)
                return ApiResponse<SectionResponse>.FailureResponse(validation.ErrorMessage!);

            // Get max order index for the course
            var maxOrder = await unitOfWork.SectionRepository.AsQueryable()
                .Where(s => s.CourseId == request.CourseId)
                .MaxAsync(s => (int?)s.OrderIndex) ?? 0;

            var section = request.ToEntity(maxOrder + 1);

            await unitOfWork.SectionRepository.AddAsync(section);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Section created: {SectionId} for course {CourseId} by user {UserId}", section.Id, request.CourseId, currentUserId);

            return ApiResponse<SectionResponse>.SuccessResponse(section.ToResponse(), "Tạo chương thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating section for course: {CourseId}", request.CourseId);
            return ApiResponse<SectionResponse>.FailureResponse("Đã xảy ra lỗi khi tạo chương.");
        }
    }

    public async Task<ApiResponse<SectionResponse>> UpdateSectionAsync(Guid sectionId, UpdateSectionRequest request, Guid currentUserId)
    {
        try
        {
            var (isValid, section, errorMessage) = await CheckSectionOwnershipAsync(sectionId, currentUserId);
            if (!isValid)
                return ApiResponse<SectionResponse>.FailureResponse(errorMessage!);

            section!.UpdateFrom(request);

            await unitOfWork.SectionRepository.UpdateAsync(sectionId, section!);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Section updated: {SectionId} by user {UserId}", sectionId, currentUserId);

            return ApiResponse<SectionResponse>.SuccessResponse(section!.ToResponse(), "Cập nhật chương thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating section: {SectionId}", sectionId);
            return ApiResponse<SectionResponse>.FailureResponse("Đã xảy ra lỗi khi cập nhật chương.");
        }
    }

    public async Task<ApiResponse<bool>> DeleteSectionAsync(Guid sectionId, Guid currentUserId)
    {
        try
        {
            var (isValid, section, errorMessage) = await CheckSectionOwnershipAsync(sectionId, currentUserId);
            if (!isValid)
                return ApiResponse<bool>.FailureResponse(errorMessage!);

            // Kiểm tra xem section có lesson không
            var lessonCount = await unitOfWork.LessonRepository.CountAsync(l => l.SectionId == sectionId && l.DeletedAt == null);
            if (lessonCount > 0)
            {
                logger.LogWarning("Cannot delete section {SectionId} with existing lessons", sectionId);
                return ApiResponse<bool>.FailureResponse("Không thể xóa chương có chứa bài học. Vui lòng xóa các bài học trước.");
            }

            // Cập nhật OrderIndex của các section còn lại
            var remainingSections = await unitOfWork.SectionRepository.AsQueryable()
                .Where(s => s.CourseId == section!.CourseId && s.Id != sectionId && s.DeletedAt == null)
                .OrderBy(s => s.OrderIndex)
                .ToListAsync();

            for (int i = 0; i < remainingSections.Count; i++)
            {
                if (remainingSections[i].OrderIndex != i + 1)
                {
                    remainingSections[i].OrderIndex = i + 1;
                    await unitOfWork.SectionRepository.UpdateAsync(remainingSections[i].Id, remainingSections[i]);
                }
            }

            section!.DeletedAt = DateTime.UtcNow;
            section.DeletedBy = currentUserId;
            await unitOfWork.SectionRepository.UpdateAsync(sectionId, section);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Section deleted: {SectionId} by user {UserId}", sectionId, currentUserId);

            return ApiResponse<bool>.SuccessResponse(true, "Xóa chương thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting section: {SectionId}", sectionId);
            return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi xóa chương.");
        }
    }

    public async Task<ApiResponse<bool>> UpdateAssignmentForSectionAsync(Guid sectionId, Guid? assignmentId, Guid currentUserId)
    {
        try
        {
            var (isValid, section, errorMessage) = await CheckSectionOwnershipAsync(sectionId, currentUserId);
            if (!isValid)
                return ApiResponse<bool>.FailureResponse(errorMessage!);

            section!.AssignmentId = assignmentId;
            await unitOfWork.SectionRepository.UpdateAsync(sectionId, section);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Section assignment updated: {SectionId} to {AssignmentId} by user {UserId}", sectionId, assignmentId, currentUserId);

            return ApiResponse<bool>.SuccessResponse(true, "Cập nhật assignment cho chương thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating section assignment: {SectionId}", sectionId);
            return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi cập nhật assignment cho chương.");
        }
    }

    public async Task<ApiResponse<bool>> SwitchSectionActivationAsync(Guid sectionId, bool isPublished, Guid currentUserId)
    {
        try
        {
            var (isValid, section, errorMessage) = await CheckSectionOwnershipAsync(sectionId, currentUserId);
            if (!isValid)
                return ApiResponse<bool>.FailureResponse(errorMessage!);

            section!.IsPublished = isPublished;
            await unitOfWork.SectionRepository.UpdateAsync(sectionId, section);

            // Cập nhật trạng thái tất cả lesson trong section
            var lessons = await unitOfWork.LessonRepository.AsQueryable()
                .Where(l => l.SectionId == sectionId && l.DeletedAt == null)
                .ToListAsync();

            foreach (var lesson in lessons)
            {
                lesson.IsPublished = isPublished;
                await unitOfWork.LessonRepository.UpdateAsync(lesson.Id, lesson);
            }

            await unitOfWork.SaveChangesAsync();

            var action = isPublished ? "hiện" : "ẩn";
            logger.LogInformation("Section activation switched: {SectionId} to {IsPublished} by user {UserId}", sectionId, isPublished, currentUserId);

            return ApiResponse<bool>.SuccessResponse(true, $"{action} chương thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error switching section activation: {SectionId}", sectionId);
            return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi chuyển đổi trạng thái kích hoạt chương.");
        }
    }

    public async Task<ApiResponse<bool>> UnlinkSectionsByAssignmentIdAsync(Guid assignmentId)
    {
        try
        {
            var sections = await unitOfWork.SectionRepository.AsQueryable()
                .Where(s => s.AssignmentId == assignmentId && s.DeletedAt == null)
                .ToListAsync();

            foreach (var section in sections)
            {
                section.AssignmentId = null;
                await unitOfWork.SectionRepository.UpdateAsync(section.Id, section);
            }

            if (sections.Count > 0)
                await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Unlinked {Count} section(s) from assignment {AssignmentId}", sections.Count, assignmentId);
            return ApiResponse<bool>.SuccessResponse(true, "Đã gỡ assignment khỏi các chương.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error unlinking sections for assignment: {AssignmentId}", assignmentId);
            return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi gỡ assignment khỏi các chương.");
        }
    }

    private async Task<(bool IsValid, string? ErrorMessage)> CheckCourseOwnershipAsync(Guid courseId, Guid currentUserId, bool isPublished = false)
    {
        var course = await unitOfWork.CourseRepository.FindOneAsync(c => c.Id == courseId && c.InstructorId == currentUserId);
        if (course == null)
        {
            logger.LogWarning("Course not found or access denied: {CourseId} for user {UserId}", courseId, currentUserId);
            return (false, "Khóa học không tồn tại hoặc bạn không có quyền truy cập.");
        }

        if (course.Status == CourseStatus.Published && isPublished)
        {
            logger.LogWarning("Cannot modify lessons in published course {CourseId} by user {UserId}", course.Id, currentUserId);
            return (false, "Không thể thêm/sửa bài học trong khóa học đã xuất bản.");
        }

        return (true, null);
    }

    private async Task<(bool IsValid, Section? Section, string? ErrorMessage)> CheckSectionOwnershipAsync(Guid sectionId, Guid currentUserId)
    {
        var section = await unitOfWork.SectionRepository.AsQueryable()
            .Include(s => s.Course)
            .FirstOrDefaultAsync(s => s.Id == sectionId);

        if (section == null)
        {
            logger.LogWarning("Section not found: {SectionId}", sectionId);
            return (false, null, "Chương không tồn tại.");
        }

        if (section.Course.InstructorId != currentUserId)
        {
            logger.LogWarning("Access denied for section {SectionId} by user {UserId}", sectionId, currentUserId);
            return (false, null, "Bạn không có quyền truy cập chương này.");
        }
        return (true, section, null);
    }
}
