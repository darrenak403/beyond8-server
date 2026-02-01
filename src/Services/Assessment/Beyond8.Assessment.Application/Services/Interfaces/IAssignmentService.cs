using Beyond8.Assessment.Application.Dtos.Assignments;
using Beyond8.Common.Utilities;

namespace Beyond8.Assessment.Application.Services.Interfaces;

public interface IAssignmentService
{
    Task<ApiResponse<AssignmentResponse>> CreateAssignmentAsync(CreateAssignmentRequest request, Guid instructorId);
}
