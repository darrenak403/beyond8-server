using System.Text.Json.Serialization;

namespace Beyond8.Integration.Application.Dtos.VnptEkyc;

public class UploadResponse
{
    [JsonPropertyName("hash")]
    public string Hash { get; set; } = string.Empty;

    [JsonPropertyName("fileName")]
    public string FileName { get; set; } = string.Empty;
}
