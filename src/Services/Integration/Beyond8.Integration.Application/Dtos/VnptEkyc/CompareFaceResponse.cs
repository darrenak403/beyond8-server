using System.Text.Json.Serialization;

namespace Beyond8.Integration.Application.Dtos.VnptEkyc
{
    public class CompareFaceResponse
    {
        [JsonPropertyName("result")]
        public string? Result { get; set; }

        [JsonPropertyName("msg")]
        public string? Msg { get; set; }

        [JsonPropertyName("prob")]
        public double Prob { get; set; }
    }
}