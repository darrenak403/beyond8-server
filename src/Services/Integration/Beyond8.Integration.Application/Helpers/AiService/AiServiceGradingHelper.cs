using System.Text;
using System.Text.Json;
using Beyond8.Integration.Application.Dtos.AiIntegration.Grading;

namespace Beyond8.Integration.Application.Helpers.AiService;

public static class AiServiceGradingHelper
{
    public static string BuildGradingPrompt(
        string template,
        string? systemPrompt,
        string assignmentTitle,
        string assignmentDescription,
        string submissionContent,
        string? rubricContent,
        int totalPoints)
    {
        var text = template
            .Replace("{AssignmentTitle}", assignmentTitle)
            .Replace("{AssignmentDescription}", assignmentDescription)
            .Replace("{SubmissionContent}", submissionContent)
            .Replace("{RubricContent}", rubricContent ?? "Không có rubric cụ thể. Chấm điểm dựa trên chất lượng tổng thể.")
            .Replace("{TotalPoints}", totalPoints.ToString());

        return string.IsNullOrWhiteSpace(systemPrompt) ? text : $"{systemPrompt}\n\n{text}";
    }

    public static string BuildSubmissionContent(string? textContent, List<string>? fileUrls)
    {
        var sb = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(textContent))
        {
            sb.AppendLine("## Nội dung bài nộp (Text):");
            sb.AppendLine(textContent);
            sb.AppendLine();
        }

        if (fileUrls != null && fileUrls.Count > 0)
        {
            sb.AppendLine("## Các file đính kèm:");
            for (int i = 0; i < fileUrls.Count; i++)
            {
                sb.AppendLine($"{i + 1}. {fileUrls[i]}");
            }
        }

        return sb.ToString();
    }

    public static AiGradingResponse? ParseGradingResponse(
        string? aiContent,
        Guid submissionId,
        int totalPoints,
        JsonSerializerOptions jsonOptions)
    {
        if (string.IsNullOrWhiteSpace(aiContent)) return null;

        var json = AiServiceJsonHelper.ExtractJson(aiContent);
        if (string.IsNullOrWhiteSpace(json)) return null;

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var response = new AiGradingResponse
            {
                SubmissionId = submissionId,
                TotalPoints = totalPoints
            };

            // Parse score
            if (root.TryGetProperty("score", out var scoreEl) && scoreEl.TryGetDecimal(out var score))
            {
                response.Score = Math.Clamp(score, 0, totalPoints);
            }
            else if (root.TryGetProperty("totalScore", out var totalScoreEl) && totalScoreEl.TryGetDecimal(out var ts))
            {
                response.Score = Math.Clamp(ts, 0, totalPoints);
            }

            // Parse summary
            response.Summary = GetStringProperty(root, "summary")
                ?? GetStringProperty(root, "overallFeedback")
                ?? "Không có nhận xét tổng quan.";

            // Parse criteria results
            response.CriteriaResults = ParseCriteriaResults(root);

            // Parse strengths
            response.Strengths = ParseStringArray(root, "strengths");

            // Parse improvements
            response.Improvements = ParseStringArray(root, "improvements")
                ?? ParseStringArray(root, "areasForImprovement")
                ?? [];

            // Parse suggestions
            response.Suggestions = ParseStringArray(root, "suggestions")
                ?? ParseStringArray(root, "recommendations")
                ?? [];

            return response;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string? GetStringProperty(JsonElement root, string propertyName)
    {
        var prop = AiServiceJsonHelper.GetPropertyIgnoreCase(root, propertyName);
        return prop?.ValueKind == JsonValueKind.String ? prop.Value.GetString() : null;
    }

    private static List<CriteriaGradingResult> ParseCriteriaResults(JsonElement root)
    {
        var results = new List<CriteriaGradingResult>();

        var criteriaEl = AiServiceJsonHelper.GetPropertyIgnoreCase(root, "criteriaResults")
            ?? AiServiceJsonHelper.GetPropertyIgnoreCase(root, "criteria")
            ?? AiServiceJsonHelper.GetPropertyIgnoreCase(root, "rubricResults");

        if (!criteriaEl.HasValue || criteriaEl.Value.ValueKind != JsonValueKind.Array)
            return results;

        foreach (var el in criteriaEl.Value.EnumerateArray())
        {
            var criteria = new CriteriaGradingResult();

            // Parse criteria name
            criteria.CriteriaName = GetElementString(el, "criteriaName")
                ?? GetElementString(el, "name")
                ?? GetElementString(el, "criteria")
                ?? "Tiêu chí";

            // Parse score
            if (el.TryGetProperty("score", out var scoreEl) && scoreEl.TryGetDecimal(out var score))
                criteria.Score = score;

            // Parse max score
            if (el.TryGetProperty("maxScore", out var maxEl) && maxEl.TryGetDecimal(out var maxScore))
                criteria.MaxScore = maxScore;
            else if (el.TryGetProperty("max", out var max2El) && max2El.TryGetDecimal(out var max2))
                criteria.MaxScore = max2;

            // Parse level
            criteria.Level = GetElementString(el, "level")
                ?? GetElementString(el, "grade")
                ?? DetermineLevel(criteria.Score, criteria.MaxScore);

            // Parse feedback
            criteria.Feedback = GetElementString(el, "feedback")
                ?? GetElementString(el, "comment")
                ?? GetElementString(el, "description")
                ?? "";

            results.Add(criteria);
        }

        return results;
    }

    private static string? GetElementString(JsonElement el, string propertyName)
    {
        return el.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.String
            ? prop.GetString()
            : null;
    }

    private static List<string> ParseStringArray(JsonElement root, string propertyName)
    {
        var list = new List<string>();
        var arr = AiServiceJsonHelper.GetPropertyIgnoreCase(root, propertyName);

        if (!arr.HasValue || arr.Value.ValueKind != JsonValueKind.Array)
            return list;

        foreach (var el in arr.Value.EnumerateArray())
        {
            var s = el.GetString();
            if (!string.IsNullOrWhiteSpace(s))
                list.Add(s);
        }

        return list;
    }

    private static string DetermineLevel(decimal score, decimal maxScore)
    {
        if (maxScore <= 0) return "N/A";

        var percentage = score / maxScore * 100;
        return percentage switch
        {
            >= 90 => "Xuất sắc",
            >= 80 => "Tốt",
            >= 70 => "Khá",
            >= 60 => "Trung bình",
            >= 50 => "Yếu",
            _ => "Kém"
        };
    }

    public static string ToFeedbackJson(AiGradingResponse response, JsonSerializerOptions jsonOptions)
    {
        var feedback = new
        {
            summary = response.Summary,
            criteriaFeedbacks = response.CriteriaResults.Select(c => new
            {
                criteriaName = c.CriteriaName,
                score = c.Score,
                maxScore = c.MaxScore,
                level = c.Level,
                feedback = c.Feedback
            }),
            strengths = response.Strengths,
            improvements = response.Improvements,
            suggestions = response.Suggestions
        };

        return JsonSerializer.Serialize(feedback, jsonOptions);
    }
}
