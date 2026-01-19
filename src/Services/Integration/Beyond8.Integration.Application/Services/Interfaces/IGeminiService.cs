using Beyond8.Common.Utilities;
using Beyond8.Integration.Application.Dtos.AiIntegration;
using Beyond8.Integration.Domain.Enums;

namespace Beyond8.Integration.Application.Services.Interfaces;

public interface IGeminiService
{
    /// <summary>
    /// Generate content using Gemini AI with a specific prompt
    /// </summary>
    Task<ApiResponse<GeminiResponse>> GenerateContentAsync(
        string prompt,
        AiOperation operation,
        Guid userId,
        Guid? promptId = null,
        string? model = null,
        int? maxTokens = null,
        decimal? temperature = null,
        decimal? topP = null);

    /// <summary>
    /// Generate content using a stored prompt template
    /// </summary>
    Task<ApiResponse<GeminiResponse>> GenerateContentWithTemplateAsync(
        Guid promptId,
        Dictionary<string, string> variables,
        AiOperation operation,
        Guid userId);

    /// <summary>
    /// Check if Gemini service is healthy and API key is valid
    /// </summary>
    Task<ApiResponse<bool>> CheckHealthAsync();
}


