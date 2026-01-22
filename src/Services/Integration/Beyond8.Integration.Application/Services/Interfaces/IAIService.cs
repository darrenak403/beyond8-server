using Beyond8.Common.Utilities;
using Beyond8.Integration.Application.Dtos.Ai;

namespace Beyond8.Integration.Application.Services.Interfaces;

public interface IAiService
{
    Task<ApiResponse<AiProfileReviewResponse>> InstructorProfileReviewAsync(ProfileReviewRequest request, Guid userId);
}
