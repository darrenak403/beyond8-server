using Beyond8.Assessment.Application.Dtos.Assignments;
using Beyond8.Common.Utilities;

namespace Beyond8.Assessment.Application.Services.Interfaces;

public interface IAssignmentService
{
    Task<ApiResponse<AssignmentSimpleResponse>> CreateAssignmentAsync(CreateAssignmentRequest request, Guid instructorId);
    Task<ApiResponse<bool>> DeleteAssignmentAsync(Guid id, Guid userId);
    Task<ApiResponse<List<AssignmentSimpleResponse>>> GetAllAssignmentsAsync(Guid userId, GetAssignmentsRequest request);
    Task<ApiResponse<AssignmentResponse>> GetAssignmentByIdAsync(Guid id, Guid userId);
    Task<ApiResponse<AssignmentResponse>> GetAssignmentByIdForStudentAsync(Guid id, Guid userId);
    Task<ApiResponse<AssignmentSimpleResponse>> UpdateAssignmentAsync(Guid id, UpdateAssignmentRequest request, Guid userId);
}
