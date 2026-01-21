using System;
using System.Text.Json.Serialization;

namespace Beyond8.Integration.Application.Dtos.VnptEkyc;

public class LivenessResponse
{
    [JsonPropertyName("img")]
    public string? Hash { get; set; } = string.Empty;

    [JsonPropertyName("fileName")]
    public string? FileName { get; set; } = string.Empty;

    [JsonPropertyName("liveness")]
    public string Liveness { get; set; } = string.Empty;

    [JsonPropertyName("liveness_msg")]
    public string LivenessMsg { get; set; } = string.Empty;

    [JsonPropertyName("face_swapping")]
    public bool FaceSwapping { get; set; }

    [JsonPropertyName("fake_liveness")]
    public bool FakeLiveness { get; set; }
}
