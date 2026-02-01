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
    public async Task<ApiResponse<AssignmentResponse>> CreateAssignmentAsync(CreateAssignmentRequest request, Guid instructorId)
    {
        try
        {
            var assignment = request.ToEntity(instructorId);
            await unitOfWork.AssignmentRepository.AddAsync(assignment);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Assignment created: {AssignmentId}, InstructorId: {InstructorId}",
                assignment.Id, instructorId);

            return ApiResponse<AssignmentResponse>.SuccessResponse(
                assignment.ToResponse(),
                "Tạo assignment thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating assignment for instructor {InstructorId}", instructorId);
            return ApiResponse<AssignmentResponse>.FailureResponse("Đã xảy ra lỗi khi tạo assignment.");
        }
    }
}
