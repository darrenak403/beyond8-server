using System.Text;
using System.Text.Json;
using Beyond8.Integration.Application.Dtos.Ai;

namespace Beyond8.Integration.Application.Helpers.AiService
{
    /// <summary>
    /// Helper xây nội dung text cho profile review và parse kết quả AI.
    /// </summary>
    public static class AiServiceProfileReviewHelper
    {
        /// <summary>
        /// Xây chuỗi mô tả hồ sơ (Bio, Headline, Education, Work, Certificates, TeachingLanguages).
        /// Phần "Các ảnh đính kèm" do caller bổ sung sau khi tải ảnh.
        /// </summary>
        public static string BuildProfileReviewText(ProfileReviewRequest r)
        {
            var sb = new StringBuilder();
            sb.AppendLine("## Bio\n" + (string.IsNullOrWhiteSpace(r.Bio) ? "(trống)" : r.Bio));
            sb.AppendLine("\n## Headline\n" + (string.IsNullOrWhiteSpace(r.Headline) ? "(trống)" : r.Headline));
            sb.AppendLine("\n## Expertise Areas\n" + (r.ExpertiseAreas?.Count > 0 ? string.Join(", ", r.ExpertiseAreas) : "(trống)"));

            sb.AppendLine("\n## Education");
            if (r.Education?.Count > 0)
                foreach (var e in r.Education)
                {
                    var fieldOfStudy = string.IsNullOrWhiteSpace(e.FieldOfStudy) ? "" : $", {e.FieldOfStudy}";
                    sb.AppendLine($"- {e.School}, {e.Degree}{fieldOfStudy} ({e.Start}-{e.End})");
                }
            else
                sb.AppendLine("(trống)");

            sb.AppendLine("\n## Work Experience");
            if (r.WorkExperience?.Count > 0)
                foreach (var w in r.WorkExperience)
                {
                    var toDate = w.IsCurrentJob ? "Hiện tại" : w.To == null ? "N/A" : w.To.Value.ToString("yyyy-MM");
                    var fromDate = w.From == DateTime.MinValue ? "N/A" : w.From.ToString("yyyy-MM");
                    var description = string.IsNullOrWhiteSpace(w.Description) ? "" : $"\n  Mô tả: {w.Description}";
                    sb.AppendLine($"- {w.Company}, {w.Role} ({fromDate} - {toDate}){description}");
                }
            else
                sb.AppendLine("(trống)");

            sb.AppendLine("\n## Certificates");
            if (r.Certificates?.Count > 0)
                foreach (var c in r.Certificates)
                    sb.AppendLine($"- {c.Name} ({c.Issuer}, {c.Year})");
            else
                sb.AppendLine("(trống)");

            sb.AppendLine("\n## Teaching Languages");
            sb.AppendLine(r.TeachingLanguages?.Count > 0 ? string.Join(", ", r.TeachingLanguages) : "(trống)");

            return sb.ToString();
        }

        /// <summary>
        /// Parse nội dung AI thành AiProfileReviewResponse. Trả về null nếu không trích được JSON hoặc deserialize thất bại.
        /// </summary>
        public static AiProfileReviewResponse? TryParseReviewResponse(string content, JsonSerializerOptions options)
        {
            var json = AiServiceJsonHelper.ExtractJson(content);
            if (string.IsNullOrWhiteSpace(json)) return null;

            try
            {
                return JsonSerializer.Deserialize<AiProfileReviewResponse>(json, options);
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }
}
