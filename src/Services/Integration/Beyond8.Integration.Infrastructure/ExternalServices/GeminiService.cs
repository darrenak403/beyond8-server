using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Beyond8.Common.Utilities;
using Beyond8.Integration.Application.Dtos.AiIntegration;
using Beyond8.Integration.Application.Services.Interfaces;
using Beyond8.Integration.Domain.Enums;
using Beyond8.Integration.Domain.Repositories.Interfaces;
using Beyond8.Integration.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Beyond8.Integration.Infrastructure.ExternalServices;

public class GeminiService(
    IOptions<GeminiConfiguration> config,
    IHttpClientFactory httpClientFactory,
    IAiUsageService aiUsageService,
    IUnitOfWork unitOfWork,
    ILogger<GeminiService> logger) : IGeminiService
{
    private readonly GeminiConfiguration _config = config.Value;

    public async Task<ApiResponse<GeminiResponse>> GenerateContentAsync(
        string prompt,
        AiOperation operation,
        Guid userId,
        Guid? promptId = null,
        string? model = null,
        int? maxTokens = null,
        decimal? temperature = null,
        decimal? topP = null,
        IReadOnlyList<GeminiImagePart>? inlineImages = null)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var selectedModel = model ?? _config.DefaultModel;
            var parts = new List<object> { new { text = prompt } };
            if (inlineImages?.Count > 0)
            {
                foreach (var img in inlineImages)
                {
                    parts.Add(new
                    {
                        inlineData = new
                        {
                            mimeType = img.MimeType,
                            data = Convert.ToBase64String(img.Data)
                        }
                    });
                }
            }

            var requestBody = new
            {
                contents = new[]
                {
                    new { parts }
                },
                generationConfig = new
                {
                    maxOutputTokens = maxTokens ?? _config.DefaultParameters.MaxTokens,
                    temperature = temperature ?? _config.DefaultParameters.Temperature,
                    topP = topP ?? _config.DefaultParameters.TopP,
                    topK = _config.DefaultParameters.TopK
                }
            };

            var httpClient = httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds);

            var url = $"{_config.ApiEndpoint}/models/{selectedModel}:generateContent?key={_config.ApiKey}";
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            var response = await httpClient.PostAsync(url, jsonContent);
            var responseContent = await response.Content.ReadAsStringAsync();

            stopwatch.Stop();

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Gemini API error: {StatusCode} - {Content}", response.StatusCode, responseContent);

                await TrackFailedUsageAsync(userId, selectedModel, operation, promptId, stopwatch.ElapsedMilliseconds, responseContent);

                return ApiResponse<GeminiResponse>.FailureResponse($"Gemini API error: {response.StatusCode}");
            }

            var geminiResponse = ParseGeminiResponse(responseContent, selectedModel, stopwatch.ElapsedMilliseconds);

            await TrackSuccessfulUsageAsync(userId, selectedModel, operation, promptId, geminiResponse, prompt);

            logger.LogInformation("Gemini content generated successfully for user {UserId} using model {Model}", userId, selectedModel);

            return ApiResponse<GeminiResponse>.SuccessResponse(geminiResponse, "Tạo nội dung AI thành công.");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(ex, "Error generating content with Gemini for user {UserId}", userId);

            await TrackFailedUsageAsync(userId, model ?? _config.DefaultModel, operation, promptId, stopwatch.ElapsedMilliseconds, ex.Message);

            return ApiResponse<GeminiResponse>.FailureResponse("Đã xảy ra lỗi khi tạo nội dung AI.");
        }
    }

    public async Task<ApiResponse<GeminiResponse>> GenerateContentWithTemplateAsync(
        Guid promptId,
        Dictionary<string, string> variables,
        AiOperation operation,
        Guid userId)
    {
        try
        {
            var promptTemplate = await unitOfWork.AiPromptRepository.GetByIdAsync(promptId);
            if (promptTemplate == null)
            {
                logger.LogWarning("Prompt template not found with ID: {PromptId}", promptId);
                return ApiResponse<GeminiResponse>.FailureResponse("Không tìm thấy prompt template.");
            }

            if (!promptTemplate.IsActive)
            {
                logger.LogWarning("Prompt template is inactive with ID: {PromptId}", promptId);
                return ApiResponse<GeminiResponse>.FailureResponse("Prompt template không hoạt động.");
            }

            var prompt = ReplaceVariables(promptTemplate.Template, variables);
            var systemPrompt = string.IsNullOrEmpty(promptTemplate.SystemPrompt)
                ? prompt
                : $"{promptTemplate.SystemPrompt}\n\n{prompt}";

            return await GenerateContentAsync(
                systemPrompt,
                operation,
                userId,
                promptId,
                null,
                promptTemplate.MaxTokens,
                promptTemplate.Temperature,
                promptTemplate.TopP);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating content with template {PromptId} for user {UserId}", promptId, userId);
            return ApiResponse<GeminiResponse>.FailureResponse("Đã xảy ra lỗi khi tạo nội dung AI từ template.");
        }
    }

    public async Task<ApiResponse<bool>> CheckHealthAsync()
    {
        try
        {
            var testPrompt = "Hello, this is a health check.";
            var result = await GenerateContentAsync(
                testPrompt,
                AiOperation.TextGeneration,
                Guid.Empty,
                null,
                null,
                10,
                0.1m,
                0.1m);

            return result.IsSuccess
                ? ApiResponse<bool>.SuccessResponse(true, "Gemini service is healthy.")
                : ApiResponse<bool>.FailureResponse("Gemini service health check failed.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking Gemini service health");
            return ApiResponse<bool>.FailureResponse("Gemini service health check failed.");
        }
    }

    private GeminiResponse ParseGeminiResponse(string responseContent, string model, long responseTimeMs)
    {
        var jsonDoc = JsonDocument.Parse(responseContent);
        var root = jsonDoc.RootElement;

        var content = root.GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString() ?? string.Empty;

        var usageMetadata = root.GetProperty("usageMetadata");
        var promptTokenCount = usageMetadata.GetProperty("promptTokenCount").GetInt32();
        var candidatesTokenCount = usageMetadata.GetProperty("candidatesTokenCount").GetInt32();
        var totalTokenCount = usageMetadata.GetProperty("totalTokenCount").GetInt32();

        var inputCost = (promptTokenCount / 1_000_000m) * _config.Pricing.InputCostPer1MTokens;
        var outputCost = (candidatesTokenCount / 1_000_000m) * _config.Pricing.OutputCostPer1MTokens;

        return new GeminiResponse
        {
            Content = content,
            InputTokens = promptTokenCount,
            OutputTokens = candidatesTokenCount,
            TotalTokens = totalTokenCount,
            InputCost = inputCost,
            OutputCost = outputCost,
            TotalCost = inputCost + outputCost,
            ResponseTimeMs = (int)responseTimeMs,
            Model = model
        };
    }

    private async Task TrackSuccessfulUsageAsync(
        Guid userId,
        string model,
        AiOperation operation,
        Guid? promptId,
        GeminiResponse response,
        string requestSummary)
    {
        var usageRequest = new AiUsageRequest
        {
            UserId = userId,
            Provider = AiProvider.Gemini,
            Model = model,
            Operation = operation,
            InputTokens = response.InputTokens,
            OutputTokens = response.OutputTokens,
            InputCost = response.InputCost,
            OutputCost = response.OutputCost,
            PromptId = promptId,
            RequestSummary = requestSummary.Length > 500 ? requestSummary[..500] : requestSummary,
            ResponseTimeMs = response.ResponseTimeMs,
            Status = AiUsageStatus.Success,
            ErrorMessage = null,
            Metadata = null
        };

        await aiUsageService.TrackUsageAsync(usageRequest);
    }

    private async Task TrackFailedUsageAsync(
        Guid userId,
        string model,
        AiOperation operation,
        Guid? promptId,
        long responseTimeMs,
        string errorMessage)
    {
        var usageRequest = new AiUsageRequest
        {
            UserId = userId,
            Provider = AiProvider.Gemini,
            Model = model,
            Operation = operation,
            InputTokens = 0,
            OutputTokens = 0,
            InputCost = 0,
            OutputCost = 0,
            PromptId = promptId,
            RequestSummary = null,
            ResponseTimeMs = (int)responseTimeMs,
            Status = AiUsageStatus.Failed,
            ErrorMessage = errorMessage.Length > 1000 ? errorMessage[..1000] : errorMessage,
            Metadata = null
        };

        await aiUsageService.TrackUsageAsync(usageRequest);
    }

    private static string ReplaceVariables(string template, Dictionary<string, string> variables)
    {
        var result = template;
        foreach (var (key, value) in variables)
        {
            result = result.Replace($"{{{key}}}", value);
        }
        return result;
    }
}
