using System.Text.Json;
using System.Text.Json.Serialization;
using Beyond8.Common.Utilities;
using Beyond8.Integration.Application.Clients;
using Beyond8.Integration.Application.Dtos.AiIntegration.Grading;
using Beyond8.Integration.Application.Dtos.AiIntegration.Profile;
using Beyond8.Integration.Application.Dtos.AiIntegration.GenerativeAi;
using Beyond8.Integration.Application.Dtos.AiIntegration.Quiz;
using Beyond8.Integration.Application.Helpers.AiService;
using Beyond8.Integration.Application.Helpers;
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
        IPdfChunkService pdfChunkService,
        IUrlContentDownloader urlContentDownloader,
        IStorageService storageService,
        IVectorEmbeddingService vectorEmbeddingService,
        IIdentityClient identityClient) : IAiService
    {
        private const int DefaultTopK = 15;
        private const string InstructorProfileReviewPromptName = "Instructor Profile Review";
        private const string QuizGenerationPromptName = "Quiz Generation";
        private const string FormatQuizQuestionsPromptName = "Format Quiz Questions";
        private const string AssignmentGradingPromptName = "Assignment Grading";
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

        public async Task<ApiResponse<List<GenQuizResponse>>> FormatQuizQuestionsFromPdfAsync(Stream stream, Guid userId)
        {
            try
            {
                var pdfText = pdfChunkService.ExtractTextFromPdf(stream);
                if (string.IsNullOrWhiteSpace(pdfText))
                    return ApiResponse<List<GenQuizResponse>>.FailureResponse("Không trích xuất được nội dung từ PDF.");

                var contentForAi = string.IsNullOrWhiteSpace(pdfText)
                    ? string.Empty
                    : pdfText.Trim().Replace("\r\n", "\n").Replace("\r", "\n");

                var promptRes = await aiPromptService.GetPromptByNameAsync(FormatQuizQuestionsPromptName);
                if (!promptRes.IsSuccess || promptRes.Data == null)
                    return ApiResponse<List<GenQuizResponse>>.FailureResponse(promptRes.Message ?? "Không tìm thấy prompt định dạng câu hỏi quiz.");

                var prompt = promptRes.Data;
                var userPrompt = prompt.Template.Replace("{Content}", contentForAi);
                string fullPrompt = string.IsNullOrEmpty(prompt.SystemPrompt) ? userPrompt : $"{prompt.SystemPrompt}\n\n{userPrompt}";

                logger.LogInformation("User prompt: {UserPrompt}", fullPrompt);

                var aiResult = await generativeAiService.GenerateContentAsync(
                    fullPrompt,
                    AiOperation.FormatQuizQuestions,
                    userId,
                    promptId: prompt.Id,
                    maxTokens: prompt.MaxTokens,
                    temperature: prompt.Temperature,
                    topP: prompt.TopP);

                if (!aiResult.IsSuccess || aiResult.Data == null)
                    return ApiResponse<List<GenQuizResponse>>.FailureResponse(
                        aiResult.Message ?? "Đã xảy ra lỗi khi định dạng câu hỏi quiz.");

                await SubscriptionHelper.UpdateUsageQuotaAsync(identityClient, userId);

                var jsonArray = AiServiceJsonHelper.ExtractJsonArray(aiResult.Data.Content);
                if (string.IsNullOrWhiteSpace(jsonArray))
                {
                    var preview = aiResult.Data.Content?.Length > 500 ? aiResult.Data.Content[..500] + "..." : aiResult.Data.Content ?? "";
                    logger.LogWarning("FormatQuizQuestions: no JSON array in response. UserId {UserId}. Preview: {Preview}", userId, preview);
                    return ApiResponse<List<GenQuizResponse>>.FailureResponse(
                        "Không thể phân tích kết quả từ AI. Kiểm tra lại file PDF.");
                }

                var questions = AiServiceQuizHelper.ParseQuestionListFromJsonArray(jsonArray);
                if (questions == null || questions.Count == 0)
                {
                    var preview = aiResult.Data.Content?.Length > 500 ? aiResult.Data.Content[..500] + "..." : aiResult.Data.Content ?? "";
                    logger.LogWarning("FormatQuizQuestions: ParseQuestionListFromJsonArray failed for UserId {UserId}. AI content preview: {Preview}", userId, preview);
                    return ApiResponse<List<GenQuizResponse>>.FailureResponse(
                        "Không thể phân tích kết quả quiz từ AI. Kiểm tra lại file PDF.");
                }

                var grouped = GroupQuestionsByDifficulty(questions);
                var parsed = new GenQuizResponse
                {
                    Easy = grouped.Easy,
                    Medium = grouped.Medium,
                    Hard = grouped.Hard
                };
                AiServiceQuizHelper.NormalizePointsToMaxPoints(parsed, questions.Count);

                return ApiResponse<List<GenQuizResponse>>.SuccessResponse([parsed], "Định dạng câu hỏi quiz từ PDF thành công.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "FormatQuizQuestions failed for UserId {UserId}", userId);
                return ApiResponse<List<GenQuizResponse>>.FailureResponse("Đã xảy ra lỗi khi định dạng câu hỏi quiz.");
            }
        }

        private static (List<QuizQuestionDto> Easy, List<QuizQuestionDto> Medium, List<QuizQuestionDto> Hard) GroupQuestionsByDifficulty(
            List<QuizQuestionDto> questions)
        {
            var easy = new List<QuizQuestionDto>();
            var medium = new List<QuizQuestionDto>();
            var hard = new List<QuizQuestionDto>();
            foreach (var q in questions)
            {
                switch (q.Difficulty)
                {
                    case DifficultyLevel.Easy:
                        easy.Add(q);
                        break;
                    case DifficultyLevel.Hard:
                        hard.Add(q);
                        break;
                    default:
                        medium.Add(q);
                        break;
                }
            }
            return (easy, medium, hard);
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

        public async Task<ApiResponse<AiGradingResponse>> AiGradingAssignmentAsync(AiGradingRequest request)
        {
            try
            {
                logger.LogInformation(
                    "Starting AI grading for submission {SubmissionId}, assignment {AssignmentId}",
                    request.SubmissionId, request.AssignmentId);

                // Get grading prompt
                var promptRes = await aiPromptService.GetPromptByNameAsync(AssignmentGradingPromptName);
                if (!promptRes.IsSuccess || promptRes.Data == null)
                    return ApiResponse<AiGradingResponse>.FailureResponse(
                        promptRes.Message ?? "Không tìm thấy prompt chấm điểm assignment.");

                var prompt = promptRes.Data;

                // Build submission content
                var submissionContent = AiServiceGradingHelper.BuildSubmissionContent(
                    request.TextContent,
                    request.FileUrls);

                if (string.IsNullOrWhiteSpace(submissionContent))
                    return ApiResponse<AiGradingResponse>.FailureResponse(
                        "Bài nộp không có nội dung để chấm điểm.");

                // Download rubric content if URL provided
                string? rubricContent = null;
                if (!string.IsNullOrWhiteSpace(request.RubricUrl))
                {
                    rubricContent = await DownloadRubricContentAsync(request.RubricUrl);
                }

                // Build grading prompt
                var fullPrompt = AiServiceGradingHelper.BuildGradingPrompt(
                    prompt.Template,
                    prompt.SystemPrompt,
                    request.AssignmentTitle,
                    request.AssignmentDescription,
                    submissionContent,
                    rubricContent,
                    request.TotalPoints);

                logger.LogDebug("Grading prompt built for submission {SubmissionId}", request.SubmissionId);

                // Call AI service
                var aiResult = await generativeAiService.GenerateContentAsync(
                    fullPrompt,
                    AiOperation.AssignmentGrading,
                    request.StudentId,
                    promptId: prompt.Id,
                    maxTokens: prompt.MaxTokens,
                    temperature: prompt.Temperature,
                    topP: prompt.TopP);

                // Update subscription quota
                // await SubscriptionHelper.UpdateUsageQuotaAsync(identityClient, request.StudentId);

                if (!aiResult.IsSuccess || aiResult.Data == null)
                {
                    logger.LogWarning(
                        "AI grading failed for submission {SubmissionId}: {Message}",
                        request.SubmissionId, aiResult.Message);
                    return ApiResponse<AiGradingResponse>.FailureResponse(
                        aiResult.Message ?? "Đã xảy ra lỗi khi chấm điểm bằng AI.");
                }

                // Parse AI response
                var gradingResult = AiServiceGradingHelper.ParseGradingResponse(
                    aiResult.Data.Content,
                    request.SubmissionId,
                    request.TotalPoints,
                    JsonOptions);

                if (gradingResult == null)
                {
                    var preview = aiResult.Data.Content?.Length > 500
                        ? aiResult.Data.Content[..500] + "..."
                        : aiResult.Data.Content ?? "";
                    logger.LogWarning(
                        "Failed to parse grading response for submission {SubmissionId}. Preview: {Preview}",
                        request.SubmissionId, preview);
                    return ApiResponse<AiGradingResponse>.FailureResponse(
                        "Không thể phân tích kết quả chấm điểm từ AI. Vui lòng thử lại.");
                }

                logger.LogInformation(
                    "AI grading completed for submission {SubmissionId}. Score: {Score}/{TotalPoints}",
                    request.SubmissionId, gradingResult.Score, gradingResult.TotalPoints);

                return ApiResponse<AiGradingResponse>.SuccessResponse(
                    gradingResult,
                    "Chấm điểm bằng AI thành công.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Error in AI grading for submission {SubmissionId}",
                    request.SubmissionId);
                return ApiResponse<AiGradingResponse>.FailureResponse(
                    "Đã xảy ra lỗi khi chấm điểm bằng AI.");
            }
        }

        private async Task<string?> DownloadRubricContentAsync(string rubricUrl)
        {
            try
            {
                if (rubricUrl.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    var key = storageService.ExtractKeyFromUrl(rubricUrl);
                    if (!string.IsNullOrEmpty(key))
                    {
                        var (data, _) = await storageService.GetObjectAsync(key);
                        if (data != null && data.Length > 0)
                        {
                            using var stream = new MemoryStream(data);
                            return pdfChunkService.ExtractTextFromPdf(stream);
                        }
                    }
                }

                var (textData, _) = await urlContentDownloader.DownloadAsync(rubricUrl);
                if (textData != null && textData.Length > 0)
                {
                    return System.Text.Encoding.UTF8.GetString(textData);
                }

                return null;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to download rubric from {RubricUrl}", rubricUrl);
                return null;
            }
        }
    }
}
