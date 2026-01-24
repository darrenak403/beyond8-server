using System.Text.Json.Serialization;

namespace Beyond8.Integration.Application.Dtos.VnptEkyc;

public class VnptEkycResponse<T>
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("object")]
    public T Object { get; set; } = default!;
}
