using System.Text.Json.Serialization;

namespace Beyond8.Integration.Application.Dtos.VnptEkyc;

/// <summary>
/// Response cho classify kèm OCR
/// Mặt trước: trả về loại giấy tờ và số giấy tờ
/// Mặt sau: trả về ngày hết hạn
/// </summary>
public class ClassifyWithOcrResponse
{

    [JsonPropertyName("type_name")]
    public string TypeName { get; set; } = string.Empty;

    [JsonPropertyName("card_name")]
    public string CardName { get; set; } = string.Empty;

    [JsonPropertyName("id_number")]
    public string? IdNumber { get; set; }

    [JsonPropertyName("issue_date")]
    public string? IssueDate { get; set; }

    [JsonPropertyName("issue_place")]
    public string? IssuePlace { get; set; }
}
