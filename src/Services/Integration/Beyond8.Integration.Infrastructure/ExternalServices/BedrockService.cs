using System.Diagnostics;
using Amazon;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Amazon.Runtime;
using Beyond8.Common.Utilities;
using Beyond8.Integration.Application.Dtos.AiIntegration.GenerativeAi;
using Beyond8.Integration.Application.Dtos.Usages;
using Beyond8.Integration.Application.Services.Interfaces;
using Beyond8.Integration.Domain.Enums;
using Beyond8.Integration.Domain.Repositories.Interfaces;
using Beyond8.Integration.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Beyond8.Integration.Infrastructure.ExternalServices
{
    public class BedrockService(
        IOptions<BedrockSettings> config,
        IAiUsageService aiUsageService,
        IUnitOfWork unitOfWork,
        ILogger<BedrockService> logger) : IGenerativeAiService
    {
        private readonly BedrockSettings _config = config.Value;

        public async Task<ApiResponse<GenerativeAiResponse>> GenerateContentAsync(
            string prompt,
            AiOperation operation,
            Guid userId,
            Guid? promptId = null,
            string? model = null,
            int? maxTokens = null,
            decimal? temperature = null,
            decimal? topP = null,
            IReadOnlyList<GenerativeAiImagePart>? inlineImages = null)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var selectedModel = model ?? _config.DefaultModel;
                var region = RegionEndpoint.GetBySystemName(_config.Region);

                if (string.IsNullOrWhiteSpace(_config.AccessKey) || string.IsNullOrWhiteSpace(_config.SecretKey))
                {
                    var errorMsg = "AWS Bedrock credentials (AccessKey and SecretKey) must be configured in appsettings.json or User Secrets. " +
                                   "Please set AWS:Bedrock:AccessKey and AWS:Bedrock:SecretKey.";
                    logger.LogError("Bedrock credentials not configured: {Error}", errorMsg);
                    return ApiResponse<GenerativeAiResponse>.FailureResponse(errorMsg);
                }

                var credentials = new BasicAWSCredentials(_config.AccessKey, _config.SecretKey);

                using var client = new AmazonBedrockRuntimeClient(credentials, region);

                var contentBlocks = new List<ContentBlock> { new() { Text = prompt } };

                if (inlineImages?.Count > 0)
                {
                    foreach (var img in inlineImages)
                    {
                        contentBlocks.Add(new ContentBlock
                        {
                            Image = new ImageBlock
                            {
                                Format = GetImageFormat(img.MimeType),
                                Source = new ImageSource
                                {
                                    Bytes = new MemoryStream(img.Data)
                                }
                            }
                        });
                    }
                }

                var request = new ConverseRequest
                {
                    ModelId = selectedModel,
                    Messages =
                    [
                        new()
                        {
                            Role = ConversationRole.User,
                            Content = contentBlocks
                        }
                    ],
                    InferenceConfig = new InferenceConfiguration
                    {
                        MaxTokens = maxTokens ?? _config.DefaultParameters.MaxTokens,
                        Temperature = (float)(temperature ?? _config.DefaultParameters.Temperature),
                        TopP = (float)(topP ?? _config.DefaultParameters.TopP)
                    }
                };

                ConverseResponse? response = null;
                var lastErrorMessage = string.Empty;

                for (var attempt = 0; attempt <= _config.MaxRetries; attempt++)
                {
                    if (attempt > 0)
                    {
                        var delayMs = GetRetryDelayMs(attempt);
                        logger.LogWarning("Bedrock throttled, retry {Attempt}/{Max} after {DelayMs}ms", attempt, _config.MaxRetries, delayMs);
                        await Task.Delay(delayMs);
                    }

                    try
                    {
                        response = await client.ConverseAsync(request);
                        break;
                    }
                    catch (ThrottlingException ex)
                    {
                        lastErrorMessage = $"Throttling: {ex.Message}";
                        logger.LogWarning(ex, "Bedrock throttling on attempt {Attempt}", attempt);
                        if (attempt >= _config.MaxRetries)
                            break;
                    }
                    catch (ValidationException ex)
                    {
                        lastErrorMessage = $"Validation error: {ex.Message}";
                        logger.LogError(ex, "Bedrock validation error");
                        break;
                    }
                    catch (AccessDeniedException ex)
                    {
                        lastErrorMessage = $"Access denied: {ex.Message}. Check AWS credentials and permissions.";
                        logger.LogError(ex, "Bedrock access denied");
                        break;
                    }
                    catch (ResourceNotFoundException ex)
                    {
                        lastErrorMessage = $"Model not found: {ex.Message}. Model may not be available in region {_config.Region}.";
                        logger.LogError(ex, "Bedrock model not found: {Model}", selectedModel);
                        break;
                    }
                    catch (Exception ex)
                    {
                        lastErrorMessage = $"Unexpected error: {ex.GetType().Name} - {ex.Message}";
                        logger.LogError(ex, "Bedrock unexpected error");
                        break;
                    }
                }

                stopwatch.Stop();

                if (response == null || response.Output?.Message?.Content == null || response.Output.Message.Content.Count == 0)
                {
                    var errorMsg = response == null
                        ? lastErrorMessage
                        : $"Invalid response: StopReason={response.StopReason}, Output={response.Output != null}";

                    logger.LogError("Bedrock API error: {Error}", errorMsg);

                    await TrackFailedUsageAsync(userId, selectedModel, operation, promptId, stopwatch.ElapsedMilliseconds, errorMsg);

                    return ApiResponse<GenerativeAiResponse>.FailureResponse($"Đã xảy ra lỗi khi gọi Bedrock API: {errorMsg}");
                }

                logger.LogInformation("Bedrock response received with StopReason: {StopReason}", response.StopReason);

                var bedrockResponse = ParseBedrockResponse(response, selectedModel, stopwatch.ElapsedMilliseconds);

                await TrackSuccessfulUsageAsync(userId, selectedModel, operation, promptId, bedrockResponse, prompt);

                logger.LogInformation("Bedrock content generated successfully for user {UserId} using model {Model}", userId, selectedModel);

                return ApiResponse<GenerativeAiResponse>.SuccessResponse(bedrockResponse, "Tạo nội dung AI thành công.");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                logger.LogError(ex, "Error generating content with Bedrock for user {UserId}", userId);

                await TrackFailedUsageAsync(userId, model ?? _config.DefaultModel, operation, promptId, stopwatch.ElapsedMilliseconds, ex.Message);

                return ApiResponse<GenerativeAiResponse>.FailureResponse("Đã xảy ra lỗi khi tạo nội dung AI.");
            }
        }

        public async Task<ApiResponse<GenerativeAiResponse>> GenerateContentWithTemplateAsync(
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
                    return ApiResponse<GenerativeAiResponse>.FailureResponse("Không tìm thấy prompt template.");
                }

                if (!promptTemplate.IsActive)
                {
                    logger.LogWarning("Prompt template is inactive with ID: {PromptId}", promptId);
                    return ApiResponse<GenerativeAiResponse>.FailureResponse("Prompt template không hoạt động.");
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
                return ApiResponse<GenerativeAiResponse>.FailureResponse("Đã xảy ra lỗi khi tạo nội dung AI từ template.");
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
                    ? ApiResponse<bool>.SuccessResponse(true, "Bedrock service is healthy.")
                    : ApiResponse<bool>.FailureResponse("Bedrock service health check failed.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error checking Bedrock service health");
                return ApiResponse<bool>.FailureResponse("Bedrock service health check failed.");
            }
        }

        private GenerativeAiResponse ParseBedrockResponse(ConverseResponse response, string model, long responseTimeMs)
        {
            var content = response.Output.Message.Content
                .Where(c => c.Text != null)
                .Select(c => c.Text)
                .FirstOrDefault() ?? string.Empty;

            var inputTokens = response.Usage.InputTokens;
            var outputTokens = response.Usage.OutputTokens;
            var totalTokens = response.Usage.TotalTokens;

            var inputCost = (inputTokens / 1_000_000m) * _config.Pricing.InputCostPer1MTokens;
            var outputCost = (outputTokens / 1_000_000m) * _config.Pricing.OutputCostPer1MTokens;

            return new GenerativeAiResponse
            {
                Content = content,
                InputTokens = inputTokens,
                OutputTokens = outputTokens,
                TotalTokens = totalTokens,
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
            GenerativeAiResponse response,
            string requestSummary)
        {
            var usageRequest = new AiUsageRequest
            {
                UserId = userId,
                Provider = AiProvider.Claude,
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
                Provider = AiProvider.Claude,
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

        private static int GetRetryDelayMs(int attempt)
        {
            var backoffMs = (int)(Math.Pow(2, attempt) * 1000);
            return Math.Min(backoffMs, 60_000);
        }

        private static string GetImageFormat(string mimeType)
        {
            return mimeType.ToLower() switch
            {
                "image/png" => "png",
                "image/jpeg" => "jpeg",
                "image/jpg" => "jpeg",
                "image/gif" => "gif",
                "image/webp" => "webp",
                _ => "jpeg"
            };
        }
    }
}
