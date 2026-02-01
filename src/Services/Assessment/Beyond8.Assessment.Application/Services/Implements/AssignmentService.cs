using Beyond8.Assessment.Application.Dtos.Assignments;
using Beyond8.Assessment.Application.Mappings.AssignmentMappings;
using Beyond8.Assessment.Application.Services.Interfaces;
using Beyond8.Assessment.Domain.Repositories.Interfaces;
using Beyond8.Common.Utilities;
using Microsoft.Extensions.Logging;

namespace Beyond8.Assessment.Application.Services.Implements;

public class AssignmentService(
    ILogger<AssignmentService> logger,
    IUnitOfWork unitOfWork) : IAssignmentService
{
    public async Task<ApiResponse<AssignmentSimpleResponse>> CreateAssignmentAsync(CreateAssignmentRequest request, Guid instructorId)
    {
        try
        {
            var assignment = request.ToEntity(instructorId);
            await unitOfWork.AssignmentRepository.AddAsync(assignment);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Assignment created: {AssignmentId}, InstructorId: {InstructorId}",
                assignment.Id, instructorId);

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

            if (assignment.SectionId != null)
            {
                return ApiResponse<bool>.FailureResponse("Không thể xóa assignment đã gắn với chương, vui lòng xóa chương trước.");
            }

            assignment.DeletedAt = DateTime.UtcNow;
            assignment.DeletedBy = userId;
            await unitOfWork.AssignmentRepository.UpdateAsync(id, assignment);
            await unitOfWork.SaveChangesAsync();

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
