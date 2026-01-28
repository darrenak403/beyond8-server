using System.Text.Json.Serialization;

namespace Beyond8.Integration.Application.Dtos.VnptEkyc
{
    public class OcrResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("dob")]
        public string? DateOfBirth { get; set; }

        [JsonPropertyName("sex")]
        public string? Sex { get; set; }

        [JsonPropertyName("nationality")]
        public string? Nationality { get; set; }

        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("issue_date")]
        public string? IssueDate { get; set; }

        [JsonPropertyName("issue_place")]
        public string? IssuePlace { get; set; }

        [JsonPropertyName("issue_loc")]
        public string? IssueLocation { get; set; }

        [JsonPropertyName("expiry_date")]
        public string? ExpiryDate { get; set; }

        [JsonPropertyName("class")]
        public string? Class { get; set; }

        [JsonPropertyName("ethnicity")]
        public string? Ethnicity { get; set; }

        [JsonPropertyName("religion")]
        public string? Religion { get; set; }

        [JsonPropertyName("features")]
        public string? Features { get; set; }

        [JsonPropertyName("passport_number")]
        public string? PassportNumber { get; set; }

        [JsonPropertyName("country_code")]
        public string? CountryCode { get; set; }

        [JsonPropertyName("place_of_birth")]
        public string? PlaceOfBirth { get; set; }

        [JsonPropertyName("full_name")]
        public string? FullName { get; set; }

        [JsonPropertyName("id_number")]
        public string? IdNumber { get; set; }
    }
}
