using System.Text.Json.Serialization;

namespace Beyond8.Integration.Application.Dtos.VnptEkyc;

public class ClassifyRequest
{
    [JsonPropertyName("img")]
    public string Img { get; set; } = string.Empty;

    [JsonPropertyName("card_type")]
    public int CardType { get; set; }
}

public class ClassifyCorrectRequest
{
    [JsonPropertyName("img_card")]
    public string ImgCard { get; set; } = string.Empty;

    [JsonPropertyName("client_session")]
    public string? ClientSession { get; set; } = string.Empty;

    [JsonPropertyName("token")]
    public string? Token { get; set; } = string.Empty;
}
