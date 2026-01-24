using System.Text.Json.Serialization;

namespace Beyond8.Integration.Application.Dtos.VnptEkyc;

public class LivenessRequest
{
    [JsonPropertyName("img")]
    public string Img { get; set; } = string.Empty;

    [JsonPropertyName("client_session")]
    public string ClientSession { get; set; } = string.Empty;
}
