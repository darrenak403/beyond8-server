using Beyond8.Assessment.Application.Dtos.AssignmentSubmissions;
using Beyond8.Common.Utilities;

namespace Beyond8.Assessment.Application.Services.Interfaces;

public interface IAssignmentSubmissionService
{
    Task<ApiResponse<SubmissionResponse>> CreateSubmissionAsync(Guid assignmentId, CreateSubmissionRequest request, Guid userId);
    Task<ApiResponse<CourseSubmissionSummaryResponse>> GetAllSubmissionsByCourseIdForInstructorAsync(Guid courseId, Guid userId);
    Task<ApiResponse<List<AssignmentSubmissionDetail>>> GetSubmissionsBySectionIdForInstructorAsync(Guid sectionId, Guid userId);
    Task<ApiResponse<SubmissionResponse>> GetSubmissionByIdAsync(Guid submissionId, Guid userId);
    Task<ApiResponse<SubmissionResponse>> GetSubmissionByIdForInstructorAsync(Guid submissionId, Guid userId);
    Task<ApiResponse<List<SubmissionResponse>>> GetSubmissionsByAssignmentIdAsync(Guid assignmentId, Guid userId);
    Task<ApiResponse<SubmissionResponse>> InstructorGradingSubmissionAsync(Guid submissionId, GradeSubmissionRequest request, Guid userId);
}
