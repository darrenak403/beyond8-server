using System.Text.Json.Serialization;

namespace Beyond8.Integration.Application.Dtos.VnptEkyc
{
    public class ClassifyWithOcrResponse
    {

        [JsonPropertyName("type_name")]
        public string TypeName { get; set; } = string.Empty;

        [JsonPropertyName("card_name")]
        public string CardName { get; set; } = string.Empty;

        [JsonPropertyName("id_number")]
        public string? IdNumber { get; set; }

        [JsonPropertyName("issue_date")]
        public string? IssueDate { get; set; }
    }
}
