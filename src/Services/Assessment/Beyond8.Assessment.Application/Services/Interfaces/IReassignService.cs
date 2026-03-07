using Beyond8.Assessment.Application.Dtos.Reassign;
using Beyond8.Common.Utilities;

namespace Beyond8.Assessment.Application.Services.Interfaces;

public interface IReassignService
{
    Task<ApiResponse<ReassignRequestResponse>> RequestQuizReassignAsync(Guid quizId, RequestQuizReassignRequest request, Guid studentId);

    Task<ApiResponse<ReassignRequestResponse>> RequestAssignmentReassignAsync(Guid assignmentId, RequestAssignmentReassignRequest request, Guid studentId);

    Task RecordQuizResetAsync(Guid quizId, Guid studentId, Guid instructorId, Guid? lessonId, int deletedCount);

    Task RecordAssignmentResetAsync(Guid assignmentId, Guid studentId, Guid instructorId, Guid? sectionId, int deletedCount);

    Task<ApiResponse<List<ReassignRequestItemDto>>> GetOverviewForInstructorAsync(Guid instructorId, GetReassignOverviewRequest request);
}
