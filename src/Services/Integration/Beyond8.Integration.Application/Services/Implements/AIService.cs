using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Beyond8.Common.Utilities;
using Beyond8.Integration.Application.Dtos.Ai;
using Beyond8.Integration.Application.Dtos.AiIntegration;
using Beyond8.Integration.Application.Services.Interfaces;
using Beyond8.Integration.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Beyond8.Integration.Application.Services.Implements;

public class AiService(
    ILogger<AiService> logger,
    IGeminiService geminiService,
    IAiPromptService aiPromptService,
    IUrlContentDownloader urlContentDownloader,
    IStorageService storageService) : IAiService
{
    private const string InstructorReviewPromptName = "Instructor Application Review";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<ApiResponse<AiInstructorApplicationReviewResponse>> InstructorApplicationReviewAsync(
        CreateInstructorProfileRequest request,
        Guid userId)
    {
        try
        {
            var promptRes = await aiPromptService.GetPromptByNameAsync(InstructorReviewPromptName);
            if (!promptRes.IsSuccess || promptRes.Data == null)
                return ApiResponse<AiInstructorApplicationReviewResponse>.FailureResponse(
                    promptRes.Message ?? "Không tìm thấy prompt đánh giá hồ sơ.");

            var t = promptRes.Data;
            var (applicationText, imageParts) = await BuildApplicationTextAndImagesAsync(request);

            var promptText = t.Template.Replace("{ApplicationText}", applicationText);
            var fullPrompt = string.IsNullOrEmpty(t.SystemPrompt) ? promptText : $"{t.SystemPrompt}\n\n{promptText}";

            var geminiResult = await geminiService.GenerateContentAsync(
                fullPrompt,
                AiOperation.InstructorApplicationReview,
                userId,
                promptId: t.Id,
                maxTokens: t.MaxTokens,
                temperature: t.Temperature,
                topP: t.TopP,
                inlineImages: imageParts.Count > 0 ? imageParts : null);

            if (!geminiResult.IsSuccess || geminiResult.Data == null)
                return ApiResponse<AiInstructorApplicationReviewResponse>.FailureResponse(
                    geminiResult.Message ?? "Đã xảy ra lỗi khi đánh giá hồ sơ.");

            var parsed = TryParseReviewResponse(geminiResult.Data.Content, userId);
            if (parsed == null)
                return ApiResponse<AiInstructorApplicationReviewResponse>.FailureResponse(
                    "Không thể phân tích kết quả đánh giá từ AI. Vui lòng thử lại.");

            return ApiResponse<AiInstructorApplicationReviewResponse>.SuccessResponse(
                parsed, "Đánh giá hồ sơ giảng viên thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in InstructorApplicationReview for user {UserId}", userId);
            return ApiResponse<AiInstructorApplicationReviewResponse>.FailureResponse(
                "Đã xảy ra lỗi khi đánh giá hồ sơ giảng viên.");
        }
    }

    private async Task<(string ApplicationText, List<GeminiImagePart> ImageParts)> BuildApplicationTextAndImagesAsync(CreateInstructorProfileRequest r)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("## Bio\n" + (string.IsNullOrWhiteSpace(r.Bio) ? "(trống)" : r.Bio));
        sb.AppendLine("\n## Headline\n" + (string.IsNullOrWhiteSpace(r.Headline) ? "(trống)" : r.Headline));
        sb.AppendLine("\n## Expertise Areas\n" + (r.ExpertiseAreas?.Count > 0 ? string.Join(", ", r.ExpertiseAreas) : "(trống)"));

        sb.AppendLine("\n## Education");
        if (r.Education?.Count > 0)
            foreach (var e in r.Education)
                sb.AppendLine($"- {e.School}, {e.Degree} ({e.Start}-{e.End})");
        else
            sb.AppendLine("(trống)");

        sb.AppendLine("\n## Work Experience");
        if (r.WorkExperience?.Count > 0)
            foreach (var w in r.WorkExperience)
                sb.AppendLine($"- {w.Company}, {w.Role} ({w.From} - {w.To})");
        else
            sb.AppendLine("(trống)");

        sb.AppendLine("\n## Identity Documents");
        if (r.IdentityDocuments?.Count > 0)
            foreach (var id in r.IdentityDocuments)
                sb.AppendLine("- Giấy tờ định danh (ảnh đính kèm)");
        else
            sb.AppendLine("(trống)");

        sb.AppendLine("\n## Certificates");
        if (r.Certificates?.Count > 0)
            foreach (var c in r.Certificates)
                sb.AppendLine($"- {c.Name} ({c.Issuer}, {c.Year})");
        else
            sb.AppendLine("(trống)");

        var imageItems = new List<(string Descriptor, string Url)>();
        foreach (var id in r.IdentityDocuments ?? [])
        {
            if (!string.IsNullOrWhiteSpace(id.FrontImg)) imageItems.Add(("CCCD mặt trước", id.FrontImg));
            if (!string.IsNullOrWhiteSpace(id.BackImg)) imageItems.Add(("CCCD mặt sau", id.BackImg));
        }
        foreach (var c in r.Certificates ?? [])
            if (!string.IsNullOrWhiteSpace(c.Url)) imageItems.Add(($"Chứng chỉ {c.Name}", c.Url));

        var imageParts = new List<GeminiImagePart>();
        var succeededDescriptors = new List<string>();
        foreach (var (desc, urlOrKey) in imageItems)
        {
            var (data, mime) = await DownloadImageAsync(urlOrKey);
            if (data == null || data.Length == 0) continue;
            imageParts.Add(new GeminiImagePart(data, mime ?? "image/jpeg"));
            succeededDescriptors.Add(desc);
        }

        sb.AppendLine("\n## Các ảnh đính kèm (theo thứ tự)");
        if (succeededDescriptors.Count > 0)
            foreach (var (d, i) in succeededDescriptors.Select((d, i) => (d, i)))
                sb.AppendLine($"{i + 1}. {d}");
        else
            sb.AppendLine("Không có ảnh nào tải được.");

        return (sb.ToString(), imageParts);
    }

    /// <summary>
    /// Ưu tiên tải từ S3 theo key (key trực tiếp hoặc rút từ URL CloudFront/S3); nếu không có key hoặc S3 lỗi thì tải qua HTTP.
    /// </summary>
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

    private AiInstructorApplicationReviewResponse? TryParseReviewResponse(string content, Guid userId)
    {
        var json = ExtractJson(content);
        if (string.IsNullOrWhiteSpace(json))
        {
            var raw = content?.Length > 800 ? content[..800] : content ?? "";
            logger.LogWarning("Failed to parse Gemini review response for user {UserId}: no JSON found. Raw (first 800 chars): {Raw}", userId, raw);
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<AiInstructorApplicationReviewResponse>(json, JsonOptions);
        }
        catch (JsonException ex)
        {
            var raw = content?.Length > 800 ? content[..800] : content ?? "";
            logger.LogWarning(ex, "Failed to parse Gemini review response for user {UserId}. Raw (first 800 chars): {Raw}", userId, raw);
            return null;
        }
    }

    /// <summary>Trích JSON từ nội dung: ưu tiên block ```json...```; nếu không có thì tìm object {...} đầu tiên (đếm ngoặc, bỏ qua trong chuỗi).</summary>
    private static string? ExtractJson(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return null;
        var s = content.Trim();
        var match = Regex.Match(s, @"```(?:json)?\s*([\s\S]*?)\s*```", RegexOptions.IgnoreCase);
        if (match.Success)
            return match.Groups[1].Value.Trim();

        var start = s.IndexOf('{');
        if (start < 0) return null;

        var inString = false;
        var escape = false;
        var depth = 1;
        for (var j = start + 1; j < s.Length; j++)
        {
            var c = s[j];
            if (escape) { escape = false; continue; }
            if (inString)
            {
                if (c == '\\') { escape = true; continue; }
                if (c == '"') { inString = false; continue; }
                continue;
            }
            if (c == '"') { inString = true; continue; }
            if (c == '{') { depth++; continue; }
            if (c == '}')
            {
                depth--;
                if (depth == 0) return s.Substring(start, j - start + 1);
            }
        }
        return null;
    }
}
