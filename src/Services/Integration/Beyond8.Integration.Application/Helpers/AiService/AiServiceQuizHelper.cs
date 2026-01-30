using System.Text;
using System.Text.Json;
using Beyond8.Integration.Application.Dtos.AiIntegration.Embedding;
using Beyond8.Integration.Application.Dtos.AiIntegration.Quiz;
using Beyond8.Integration.Domain.Enums;

namespace Beyond8.Integration.Application.Helpers.AiService
{
    public static class AiServiceQuizHelper
    {
        public static string BuildQuizContextText(List<VectorSearchResult> chunks)
        {
            var sb = new StringBuilder();
            foreach (var c in chunks)
                sb.AppendLine(c.Text);
            return sb.ToString();
        }

        public static string BuildQuizPromptFromTemplate(
            string template,
            string? systemPrompt,
            string contextText,
            string queryPart,
            int easyCount,
            int mediumCount,
            int hardCount,
            int maxPoints)
        {
            var text = template
                .Replace("{Context}", contextText)
                .Replace("{Query}", queryPart)
                .Replace("{EasyCount}", easyCount.ToString())
                .Replace("{MediumCount}", mediumCount.ToString())
                .Replace("{HardCount}", hardCount.ToString())
                .Replace("{MaxPoints}", maxPoints.ToString());
            return string.IsNullOrWhiteSpace(systemPrompt) ? text : $"{systemPrompt}\n\n{text}";
        }

        public static (int easyCount, int mediumCount, int hardCount) CalculateQuestionCounts(
            int totalCount,
            DifficultyDistribution distribution)
        {
            var easyCount = (int)Math.Round(totalCount * distribution.EasyPercent / 100.0);
            var mediumCount = (int)Math.Round(totalCount * distribution.MediumPercent / 100.0);
            var hardCount = totalCount - easyCount - mediumCount;
            return (easyCount, mediumCount, hardCount);
        }

        public static GenQuizResponse? ParseQuizResponse(
            string aiContent,
            GenQuizRequest request,
            JsonSerializerOptions jsonOptions)
        {
            var json = AiServiceJsonHelper.ExtractJson(aiContent);
            if (string.IsNullOrWhiteSpace(json)) return null;

            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                var easy = ParseQuestionArrayFromElement(
                    AiServiceJsonHelper.GetPropertyIgnoreCase(root, "easy"), DifficultyLevel.Easy);
                var medium = ParseQuestionArrayFromElement(
                    AiServiceJsonHelper.GetPropertyIgnoreCase(root, "medium"), DifficultyLevel.Medium);
                var hard = ParseQuestionArrayFromElement(
                    AiServiceJsonHelper.GetPropertyIgnoreCase(root, "hard"), DifficultyLevel.Hard);
                return new GenQuizResponse
                {
                    CourseId = request.CourseId,
                    Query = request.Query,
                    Easy = easy,
                    Medium = medium,
                    Hard = hard
                };
            }
            catch (JsonException)
            {
                return null;
            }
        }

        public static List<QuizQuestionDto> ParseQuestionArrayFromElement(
            JsonElement? arr,
            DifficultyLevel defaultLevel)
        {
            var list = new List<QuizQuestionDto>();
            if (!arr.HasValue || arr.Value.ValueKind != JsonValueKind.Array) return list;
            foreach (var el in arr.Value.EnumerateArray())
            {
                var q = ParseOneQuestion(el, defaultLevel);
                if (q != null) list.Add(q);
            }
            return list;
        }

        public static QuizQuestionDto? ParseOneQuestion(JsonElement el, DifficultyLevel defaultLevel)
        {
            if (!el.TryGetProperty("content", out var contentEl)) return null;
            var content = contentEl.GetString();
            if (string.IsNullOrWhiteSpace(content)) return null;

            var options = new List<QuestionOptionItem>();
            if (el.TryGetProperty("options", out var opts) && opts.ValueKind == JsonValueKind.Array)
            {
                foreach (var o in opts.EnumerateArray())
                {
                    var id = o.TryGetProperty("id", out var idEl) ? idEl.GetString() ?? "" : "";
                    var text = o.TryGetProperty("text", out var textEl) ? textEl.GetString() ?? "" : "";
                    var isCorrect = o.TryGetProperty("isCorrect", out var cEl) && cEl.ValueKind == JsonValueKind.True;
                    options.Add(new QuestionOptionItem { Id = id, Text = text, IsCorrect = isCorrect });
                }
            }

            var explanation = el.TryGetProperty("explanation", out var exEl) ? exEl.GetString() : null;
            var tags = new List<string>();
            if (el.TryGetProperty("tags", out var tagsEl) && tagsEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var t in tagsEl.EnumerateArray())
                {
                    var s = t.GetString();
                    if (!string.IsNullOrEmpty(s)) tags.Add(s);
                }
            }

            var difficulty = defaultLevel;
            if (el.TryGetProperty("difficulty", out var dEl) && dEl.TryGetInt32(out var d))
                difficulty = (DifficultyLevel)Math.Clamp(d, 0, 2);

            var points = 1m;
            if (el.TryGetProperty("points", out var pEl) && pEl.ValueKind == JsonValueKind.Number && pEl.TryGetDecimal(out var pd))
                points = Math.Max(0.5m, pd);

            return new QuizQuestionDto
            {
                Content = content,
                Type = QuestionType.MultipleChoice,
                Options = options,
                Explanation = explanation,
                Tags = tags,
                Difficulty = difficulty,
                Points = points
            };
        }

        /// <summary>
        /// Chuẩn hóa điểm từng câu sao cho tổng điểm đúng bằng maxPoints (khắc phục AI trả về tổng lệch).
        /// </summary>
        public static void NormalizePointsToMaxPoints(GenQuizResponse response, int maxPoints)
        {
            var all = new List<QuizQuestionDto>();
            all.AddRange(response.Easy);
            all.AddRange(response.Medium);
            all.AddRange(response.Hard);
            if (all.Count == 0) return;

            var currentSum = all.Sum(q => q.Points);
            if (currentSum <= 0)
            {
                var perQuestion = (decimal)maxPoints / all.Count;
                var rounded = Math.Round(perQuestion, 1);
                foreach (var q in all)
                    q.Points = rounded;
                var diff = maxPoints - all.Sum(q => q.Points);
                if (diff != 0 && all.Count > 0)
                    all[0].Points += diff;
                return;
            }

            var factor = (decimal)maxPoints / currentSum;
            foreach (var q in all)
                q.Points = Math.Max(0.5m, Math.Round(q.Points * factor, 1));

            var total = all.Sum(q => q.Points);
            var delta = maxPoints - total;
            if (delta == 0) return;
            all[0].Points = Math.Max(0.5m, all[0].Points + delta);
        }
    }
}
