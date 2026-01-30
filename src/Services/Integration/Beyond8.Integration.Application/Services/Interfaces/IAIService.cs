using Beyond8.Common.Utilities;
using Beyond8.Integration.Application.Dtos.AiIntegration.Profile;
using Beyond8.Integration.Application.Dtos.AiIntegration.Quiz;

namespace Beyond8.Integration.Application.Services.Interfaces
{
    public interface IAiService
    {
        Task<ApiResponse<AiProfileReviewResponse>> InstructorProfileReviewAsync(ProfileReviewRequest request, Guid userId);

        Task<ApiResponse<GenQuizResponse>> GenerateQuizAsync(
            GenQuizRequest request,
            Guid userId,
            CancellationToken cancellationToken = default);
    }
}
