using System.Text.Json;
using System.Text.Json.Serialization;

namespace Beyond8.Integration.Application.Dtos.AiIntegration.Profile
{
    public class AiProfileReviewResponse
    {
        private List<SectionDetail> _details = [];

        public bool IsAccepted { get; set; }
        public double TotalScore { get; set; }
        public string? FeedbackSummary { get; set; }
        public List<SectionDetail> Details { get => _details; set => _details = value ?? []; }
        public string? AdditionalFeedback { get; set; }
    }

    public class SectionDetail
    {
        private List<string> _issues = [];
        private List<string> _suggestions = [];

        public string SectionName { get; set; } = string.Empty;

        [JsonConverter(typeof(SectionStatusJsonConverter))]
        public SectionStatus Status { get; set; }
        public double Score { get; set; }
        public List<string> Issues { get => _issues; set => _issues = value ?? []; }
        public List<string> Suggestions { get => _suggestions; set => _suggestions = value ?? []; }
    }

    public sealed class SectionStatusJsonConverter : JsonConverter<SectionStatus>
    {
        public override SectionStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String) return default;
            var s = reader.GetString();
            return Enum.TryParse<SectionStatus>(s, ignoreCase: true, out var v) ? v : default;
        }

        public override void Write(Utf8JsonWriter writer, SectionStatus value, JsonSerializerOptions options) =>
            writer.WriteStringValue(value.ToString());
    }

    public enum SectionStatus
    {
        Valid,
        Warning,
        Invalid
    }
}