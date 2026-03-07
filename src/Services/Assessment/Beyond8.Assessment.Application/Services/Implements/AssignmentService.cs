using Beyond8.Assessment.Application.Clients.Catalog;
using Beyond8.Assessment.Application.Clients.Learning;
using Beyond8.Assessment.Application.Dtos.Assignments;
using Beyond8.Assessment.Application.Mappings.AssignmentMappings;
using Beyond8.Assessment.Application.Services.Interfaces;
using Beyond8.Assessment.Domain.Repositories.Interfaces;
using Beyond8.Common.Events.Assessment;
using Beyond8.Common.Utilities;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Assessment.Application.Services.Implements;

public class AssignmentService(
    ILogger<AssignmentService> logger,
    IUnitOfWork unitOfWork,
    ICatalogService catalogService,
    ILearningClient learningClient,
    IPublishEndpoint publishEndpoint) : IAssignmentService
{
    public async Task<ApiResponse<AssignmentResponse>> GetAssignmentByIdForStudentAsync(Guid id, Guid userId)
    {
        try
        {
            var assignment = await unitOfWork.AssignmentRepository.FindOneAsync(a => a.Id == id);
            if (assignment == null)
            {
                logger.LogError("Assignment not found for id: {Id} for student", id);
                return ApiResponse<AssignmentResponse>.FailureResponse("Assignment không tồn tại cho học sinh.");
            }

            if (!assignment.CourseId.HasValue)
            {
                logger.LogWarning("Assignment {AssignmentId} has no CourseId, denying student access", id);
                return ApiResponse<AssignmentResponse>.FailureResponse("Assignment không gắn khóa học. Không thể truy cập.");
            }

            var enrollmentResult = await learningClient.IsUserEnrolledInCourseAsync(assignment.CourseId.Value);
            if (!enrollmentResult.IsSuccess)
            {
                logger.LogWarning("Learning client failed for assignment {AssignmentId}, course {CourseId}: {Message}", id, assignment.CourseId, enrollmentResult.Message);
                return ApiResponse<AssignmentResponse>.FailureResponse(enrollmentResult.Message ?? "Không thể kiểm tra đăng ký khóa học.");
            }

            if (!enrollmentResult.Data)
            {
                logger.LogWarning("Student not enrolled in course {CourseId} for assignment {AssignmentId}", assignment.CourseId, id);
                return ApiResponse<AssignmentResponse>.FailureResponse("Bạn chưa đăng ký khóa học. Vui lòng đăng ký khóa học trước khi xem bài tập.");
            }

            return ApiResponse<AssignmentResponse>.SuccessResponse(assignment.ToResponse(), "Lấy assignment thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting assignment by id for student: {Id}", id);
            return ApiResponse<AssignmentResponse>.FailureResponse("Đã xảy ra lỗi khi lấy assignment cho học sinh.");
        }
    }

    public async Task<ApiResponse<AssignmentSimpleResponse>> CreateAssignmentAsync(CreateAssignmentRequest request, Guid instructorId)
    {
        try
        {
            var assignment = request.ToEntity(instructorId);
            await unitOfWork.AssignmentRepository.AddAsync(assignment);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Assignment created: {AssignmentId}, InstructorId: {InstructorId}",
                assignment.Id, instructorId);

            if (assignment.SectionId != null)
            {
                var catalogResponse = await catalogService.UpdateAssignmentForSectionAsync(assignment.SectionId.Value, assignment.Id);
                if (!catalogResponse.IsSuccess)
                {
                    logger.LogError("Error updating assignment for section: {SectionId}", assignment.SectionId.Value);
                    return ApiResponse<AssignmentSimpleResponse>.FailureResponse(catalogResponse.Message!);
                }
            }

            return ApiResponse<AssignmentSimpleResponse>.SuccessResponse(
                assignment.ToSimpleResponse(),
                "Tạo assignment thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating assignment for instructor {InstructorId}", instructorId);
            return ApiResponse<AssignmentSimpleResponse>.FailureResponse("Đã xảy ra lỗi khi tạo assignment.");
        }
    }

    public async Task<ApiResponse<AssignmentSimpleResponse>> UpdateAssignmentAsync(Guid id, UpdateAssignmentRequest request, Guid userId)
    {
        try
        {
            var assignment = await unitOfWork.AssignmentRepository.FindOneAsync(a => a.Id == id && a.InstructorId == userId);
            if (assignment == null)
                return ApiResponse<AssignmentSimpleResponse>.FailureResponse("Assignment không tồn tại hoặc không thuộc về người dùng.");

            assignment.UpdateFromRequest(request);
            await unitOfWork.AssignmentRepository.UpdateAsync(id, assignment);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Assignment updated: {AssignmentId}, InstructorId: {UserId}", id, userId);
            return ApiResponse<AssignmentSimpleResponse>.SuccessResponse(
                assignment.ToSimpleResponse(),
                "Cập nhật assignment thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating assignment: {Id}", id);
            return ApiResponse<AssignmentSimpleResponse>.FailureResponse("Đã xảy ra lỗi khi cập nhật assignment.");
        }
    }

    public async Task<ApiResponse<AssignmentResponse>> GetAssignmentByIdAsync(Guid id, Guid userId)
    {
        try
        {
            var assignment = await unitOfWork.AssignmentRepository.FindOneAsync(a => a.Id == id && a.InstructorId == userId);
            if (assignment == null)
            {
                return ApiResponse<AssignmentResponse>.FailureResponse("Assignment không tồn tại hoặc không thuộc về người dùng.");
            }

            return ApiResponse<AssignmentResponse>.SuccessResponse(assignment.ToResponse(), "Lấy assignment thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting assignment by id: {Id}", id);
            return ApiResponse<AssignmentResponse>.FailureResponse("Đã xảy ra lỗi khi lấy assignment.");
        }
    }

    public async Task<ApiResponse<List<AssignmentSimpleResponse>>> GetAllAssignmentsAsync(Guid userId, GetAssignmentsRequest request)
    {
        try
        {
            var assignments = await unitOfWork.AssignmentRepository.GetPagedAsync(
                pageNumber: request.PageNumber,
                pageSize: request.PageSize,
                filter: a => a.InstructorId == userId
                    && (!request.CourseId.HasValue || a.CourseId == request.CourseId)
                    && (!request.SectionId.HasValue || a.SectionId == request.SectionId),
                orderBy: query => query.OrderByDescending(a => a.CreatedAt));

            return ApiResponse<List<AssignmentSimpleResponse>>.SuccessPagedResponse(
                [.. assignments.Items.Select(a => a.ToSimpleResponse())],
                assignments.TotalCount,
                request.PageNumber,
                request.PageSize,
                "Lấy tất cả assignments thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting all assignments for instructor: {InstructorId}", userId);
            return ApiResponse<List<AssignmentSimpleResponse>>.FailureResponse("Đã xảy ra lỗi khi lấy tất cả assignments.");
        }
    }

    public async Task<ApiResponse<bool>> DeleteAssignmentAsync(Guid id, Guid userId)
    {
        try
        {
            var assignment = await unitOfWork.AssignmentRepository.FindOneAsync(a => a.Id == id && a.InstructorId == userId);
            if (assignment == null)
            {
                return ApiResponse<bool>.FailureResponse("Assignment không tồn tại hoặc không thuộc về người dùng.");
            }

            assignment.DeletedAt = DateTime.UtcNow;
            assignment.DeletedBy = userId;
            await unitOfWork.AssignmentRepository.UpdateAsync(id, assignment);
            await unitOfWork.SaveChangesAsync();

            await publishEndpoint.Publish(new AssignmentDeletedEvent(id));

            logger.LogInformation("Assignment deleted: {AssignmentId}, InstructorId: {InstructorId}", id, userId);
            return ApiResponse<bool>.SuccessResponse(true, "Xóa assignment thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting assignment: {Id}", id);
            return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi xóa assignment.");
        }
    }
}
