using Beyond8.Common.Utilities;
using Beyond8.Integration.Application.Dtos.AiIntegration.GenerativeAi;
using Beyond8.Integration.Domain.Enums;

namespace Beyond8.Integration.Application.Services.Interfaces
{
    public interface IGenerativeAiService
    {
        Task<ApiResponse<GenerativeAiResponse>> GenerateContentAsync(
            string prompt,
            AiOperation operation,
            Guid userId,
            Guid? promptId = null,
            string? model = null,
            int? maxTokens = null,
            decimal? temperature = null,
            decimal? topP = null,
            IReadOnlyList<GenerativeAiImagePart>? inlineImages = null);

        Task<ApiResponse<GenerativeAiResponse>> GenerateContentWithTemplateAsync(
            Guid promptId,
            Dictionary<string, string> variables,
            AiOperation operation,
            Guid userId);

        Task<ApiResponse<bool>> CheckHealthAsync();
    }
}
