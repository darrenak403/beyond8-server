using System.Text.Json.Serialization;

namespace Beyond8.Integration.Application.Dtos.VnptEkyc
{
    public class OcrRequestBack
    {
        [JsonPropertyName("img_back")]
        public string ImgBack { get; set; } = string.Empty;

        [JsonPropertyName("client_session")]
        public string ClientSession { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public int Type { get; set; } = 1;

        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;
    }
}
