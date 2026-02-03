using System;
using Beyond8.Assessment.Application.Dtos.AssignmentSubmissions;
using Beyond8.Common.Utilities;

namespace Beyond8.Assessment.Application.Services.Interfaces;

public interface IAssignmentSubmissionService
{
    Task<ApiResponse<SubmissionResponse>> CreateSubmissionAsync(Guid assignmentId, CreateSubmissionRequest request, Guid userId);
}
