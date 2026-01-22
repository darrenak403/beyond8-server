using System.Text.Json.Serialization;

namespace Beyond8.Integration.Application.Dtos.VnptEkyc;


public class ClassifySuccessResponse
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = "IDG-00000000";

    [JsonPropertyName("object")]
    public IdObject Object { get; set; } = new IdObject();
}

public class IdObject
{
    [JsonPropertyName("type")]
    public int Type { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
