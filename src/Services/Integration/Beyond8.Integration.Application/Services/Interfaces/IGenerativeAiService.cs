using Beyond8.Common.Utilities;
using Beyond8.Integration.Application.Dtos.AiIntegration;
using Beyond8.Integration.Domain.Enums;

namespace Beyond8.Integration.Application.Services.Interfaces;

public interface IGenerativeAiService
{
    /// <summary>
    /// Generate content using AI with a specific prompt. Supports images/PDF inline (multimodal).
    /// </summary>
    /// <param name="inlineImages">Images/PDF to send (base64). Order matches description in prompt.</param>
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

    /// <summary>
    /// Generate content using a stored prompt template
    /// </summary>
    Task<ApiResponse<GenerativeAiResponse>> GenerateContentWithTemplateAsync(
        Guid promptId,
        Dictionary<string, string> variables,
        AiOperation operation,
        Guid userId);

    /// <summary>
    /// Check if AI service is healthy and API key is valid
    /// </summary>
    Task<ApiResponse<bool>> CheckHealthAsync();
}
