using System.Text.Json.Serialization;

namespace Beyond8.Integration.Application.Dtos.VnptEkyc
{
    public class OcrRequestFront
    {
        [JsonPropertyName("img_front")]
        public string ImgFront { get; set; } = string.Empty;

        [JsonPropertyName("client_session")]
        public string ClientSession { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public int Type { get; set; } = 1;

        [JsonPropertyName("validate_postcode")]
        public bool? ValidatePostcode { get; set; }

        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;
    }
}
