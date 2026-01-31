using System.Text.Json;
using System.Text.Json.Serialization;
using Beyond8.Common.Utilities;
using Beyond8.Integration.Application.Clients;
using Beyond8.Integration.Application.Dtos.AiIntegration.Profile;
using Beyond8.Integration.Application.Dtos.AiIntegration.GenerativeAi;
using Beyond8.Integration.Application.Dtos.AiIntegration.Quiz;
using Beyond8.Integration.Application.Helpers.AiService;
using Beyond8.Integration.Application.Mappings.AiIntegrationMappings;
using Beyond8.Integration.Application.Services.Interfaces;
using Beyond8.Integration.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Beyond8.Integration.Application.Services.Implements
{
    public class AiService(
        ILogger<AiService> logger,
        IGenerativeAiService generativeAiService,
        IAiPromptService aiPromptService,
        IUrlContentDownloader urlContentDownloader,
        IStorageService storageService,
        IVectorEmbeddingService vectorEmbeddingService,
        IIdentityClient identityClient) : IAiService
    {
        private const int DefaultTopK = 15;
        private const string InstructorProfileReviewPromptName = "Instructor Profile Review";
        private const string QuizGenerationPromptName = "Quiz Generation";
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };

        public async Task<ApiResponse<AiProfileReviewResponse>> InstructorProfileReviewAsync(
            ProfileReviewRequest request,
            Guid userId)
        {
            try
            {
                var promptRes = await aiPromptService.GetPromptByNameAsync(InstructorProfileReviewPromptName);
                if (!promptRes.IsSuccess || promptRes.Data == null)
                    return ApiResponse<AiProfileReviewResponse>.FailureResponse(promptRes.Message ?? "Không tìm thấy prompt đánh giá hồ sơ.");

                var t = promptRes.Data;
                var (profileReviewText, imageParts) = await BuildProfileReviewTextAndImagesAsync(request);

                var promptText = t.Template.Replace("{ProfileReviewText}", profileReviewText);
                var fullPrompt = string.IsNullOrEmpty(t.SystemPrompt) ? promptText : $"{t.SystemPrompt}\n\n{promptText}";

                var geminiResult = await generativeAiService.GenerateContentAsync(
                    fullPrompt,
                    AiOperation.ProfileReview,
                    userId,
                    promptId: t.Id,
                    maxTokens: t.MaxTokens,
                    temperature: t.Temperature,
                    topP: t.TopP,
                    inlineImages: imageParts.Count > 0 ? imageParts : null);

                await SubscriptionHelper.UpdateUsageQuotaAsync(identityClient, userId);

                if (!geminiResult.IsSuccess || geminiResult.Data == null)
                    return ApiResponse<AiProfileReviewResponse>.FailureResponse(geminiResult.Message ?? "Đã xảy ra lỗi khi đánh giá hồ sơ.");

                var parsed = AiServiceProfileReviewHelper.TryParseReviewResponse(geminiResult.Data.Content, JsonOptions);
                if (parsed == null)
                {
                    var raw = geminiResult.Data.Content?.Length > 800 ? geminiResult.Data.Content[..800] : geminiResult.Data.Content ?? "";
                    logger.LogWarning("Failed to parse review response for user {UserId}: no valid JSON. Raw (first 800): {Raw}", userId, raw);
                    return ApiResponse<AiProfileReviewResponse>.FailureResponse("Không thể phân tích kết quả đánh giá từ AI. Vui lòng thử lại.");
                }

                return ApiResponse<AiProfileReviewResponse>.SuccessResponse(parsed, "Đánh giá hồ sơ giảng viên thành công.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in InstructorApplicationReview for user {UserId}", userId);
                return ApiResponse<AiProfileReviewResponse>.FailureResponse("Đã xảy ra lỗi khi đánh giá hồ sơ giảng viên.");
            }
        }

        public async Task<ApiResponse<GenQuizResponse>> GenerateQuizAsync(
            GenQuizRequest request,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var promptRes = await aiPromptService.GetPromptByNameAsync(QuizGenerationPromptName);
                if (!promptRes.IsSuccess || promptRes.Data == null)
                    return ApiResponse<GenQuizResponse>.FailureResponse(promptRes.Message ?? "Không tìm thấy prompt tạo câu hỏi tự động.");

                var searchRequest = request.ToVectorSearchRequest(DefaultTopK);
                var contextChunks = await vectorEmbeddingService.SearchAsync(searchRequest);
                if (contextChunks == null || contextChunks.Count == 0)
                    return ApiResponse<GenQuizResponse>.FailureResponse("Không tìm thấy ngữ cảnh phù hợp cho khóa học này. Vui lòng thêm tài liệu vào khóa học trước.");

                var contextText = AiServiceQuizHelper.BuildQuizContextText(contextChunks);
                var distribution = request.Distribution ?? new DifficultyDistribution();
                var (easyCount, mediumCount, hardCount) = AiServiceQuizHelper.CalculateQuestionCounts(request.TotalCount, distribution);
                var queryPart = string.IsNullOrWhiteSpace(request.Query) ? "(toàn bộ khóa học)" : request.Query;

                var prompt = promptRes.Data;
                var userPrompt = AiServiceQuizHelper.BuildQuizPromptFromTemplate(
                    prompt.Template,
                    prompt.SystemPrompt,
                    contextText,
                    queryPart,
                    easyCount,
                    mediumCount,
                    hardCount,
                    request.MaxPoints);

                logger.LogInformation("User prompt: {UserPrompt}", userPrompt);

                var aiResult = await generativeAiService.GenerateContentAsync(
                    userPrompt,
                    AiOperation.QuizGeneration,
                    userId,
                    promptId: prompt.Id,
                    maxTokens: prompt.MaxTokens,
                    temperature: prompt.Temperature,
                    topP: prompt.TopP);

                await SubscriptionHelper.UpdateUsageQuotaAsync(identityClient, userId);

                if (!aiResult.IsSuccess || aiResult.Data == null)
                    return ApiResponse<GenQuizResponse>.FailureResponse(
                        aiResult.Message ?? "Không thể sinh quiz. Vui lòng thử lại.");

                var parsed = AiServiceQuizHelper.ParseQuizResponse(aiResult.Data.Content, request, JsonOptions);
                if (parsed == null)
                {
                    var preview = aiResult.Data.Content?.Length > 500 ? aiResult.Data.Content[..500] + "..." : aiResult.Data.Content ?? "";
                    logger.LogWarning("ParseQuizResponse failed for CourseId {CourseId}. AI content preview: {Preview}", request.CourseId, preview);
                    return ApiResponse<GenQuizResponse>.FailureResponse("Không thể phân tích kết quả quiz từ AI. Kiểm tra lại thông tin.");
                }

                AiServiceQuizHelper.NormalizePointsToMaxPoints(parsed, request.MaxPoints);

                return ApiResponse<GenQuizResponse>.SuccessResponse(parsed, "Tạo quiz từ AI thành công.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "GenerateQuiz failed for CourseId {CourseId}, UserId {UserId}",
                    request.CourseId, userId);
                return ApiResponse<GenQuizResponse>.FailureResponse(
                    "Đã xảy ra lỗi khi tạo quiz từ AI.");
            }
        }

        private async Task<(string ProfileReviewText, List<GenerativeAiImagePart> ImageParts)> BuildProfileReviewTextAndImagesAsync(ProfileReviewRequest r)
        {
            var (profileText, imageItems) = AiServiceProfileReviewHelper.BuildProfileReviewText(r);

            var imageParts = new List<GenerativeAiImagePart>();
            var succeededDescriptors = new List<string>();
            foreach (var (desc, urlOrKey) in imageItems)
            {
                var (data, mime) = await DownloadImageAsync(urlOrKey);
                if (data == null || data.Length == 0) continue;
                imageParts.Add(new GenerativeAiImagePart(data, mime ?? "image/jpeg"));
                succeededDescriptors.Add(desc);
            }

            var sb = new System.Text.StringBuilder(profileText);
            sb.AppendLine("\n## Các ảnh đính kèm (theo thứ tự)");
            if (succeededDescriptors.Count > 0)
                foreach (var (d, i) in succeededDescriptors.Select((d, i) => (d, i)))
                    sb.AppendLine($"{i + 1}. {d}");
            else
                sb.AppendLine("Không có ảnh nào tải được.");

            var fullText = sb.ToString();
            logger.LogInformation("Application text: {ApplicationText}", fullText);
            logger.LogInformation("Image parts: {ImageParts}", imageParts.Count);

            return (fullText, imageParts);
        }

        private async Task<(byte[]? Data, string? MimeType)> DownloadImageAsync(string urlOrKey)
        {
            if (string.IsNullOrWhiteSpace(urlOrKey)) return (null, null);

            var isUrl = urlOrKey.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                        || urlOrKey.StartsWith("https://", StringComparison.OrdinalIgnoreCase);

            if (isUrl)
            {
                var key = storageService.ExtractKeyFromUrl(urlOrKey);
                if (!string.IsNullOrEmpty(key))
                {
                    var (data, contentType) = await storageService.GetObjectAsync(key);
                    if (data != null && data.Length > 0)
                        return (data, contentType ?? "image/jpeg");
                }
                return await urlContentDownloader.DownloadAsync(urlOrKey);
            }

            var (data2, ct) = await storageService.GetObjectAsync(urlOrKey);
            return (data2, ct ?? "image/jpeg");
        }
    }
}
