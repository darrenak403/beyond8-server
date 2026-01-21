using Beyond8.Common.Utilities;
using Beyond8.Integration.Application.Dtos.Ai;

namespace Beyond8.Integration.Application.Services.Interfaces;

public interface IAiService
{
    Task<ApiResponse<AiInstructorApplicationReviewResponse>> InstructorApplicationReviewAsync(CreateInstructorProfileRequest request, Guid userId);
}
