using System.Text.Json.Serialization;

namespace Beyond8.Integration.Application.Dtos.VnptEkyc;

/// <summary>
/// Response từ OCR endpoint (có thể là mặt trước hoặc mặt sau)
/// </summary>
public class OcrResponse
{
    /// <summary>
    /// Số CMND/CCCD (thường có ở mặt trước)
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// Họ và tên
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Ngày sinh (format: dd/MM/yyyy hoặc yyyy-MM-dd)
    /// </summary>
    [JsonPropertyName("dob")]
    public string? DateOfBirth { get; set; }

    /// <summary>
    /// Giới tính (NAM/NỮ hoặc M/F)
    /// </summary>
    [JsonPropertyName("sex")]
    public string? Sex { get; set; }

    /// <summary>
    /// Quốc tịch
    /// </summary>
    [JsonPropertyName("nationality")]
    public string? Nationality { get; set; }

    /// <summary>
    /// Địa chỉ thường trú
    /// </summary>
    [JsonPropertyName("address")]
    public string? Address { get; set; }

    /// <summary>
    /// Loại giấy tờ
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// Ngày cấp (format: dd/MM/yyyy hoặc yyyy-MM-dd)
    /// </summary>
    [JsonPropertyName("issue_date")]
    public string? IssueDate { get; set; }

    /// <summary>
    /// Nơi cấp (issue_place trong response VNPT)
    /// </summary>
    [JsonPropertyName("issue_place")]
    public string? IssuePlace { get; set; }

    /// <summary>
    /// Nơi cấp (alias cho issue_place, giữ lại để backward compatibility)
    /// </summary>
    [JsonPropertyName("issue_loc")]
    public string? IssueLocation { get; set; }

    /// <summary>
    /// Ngày hết hạn (format: dd/MM/yyyy hoặc yyyy-MM-dd) - thường có ở mặt sau
    /// </summary>
    [JsonPropertyName("expiry_date")]
    public string? ExpiryDate { get; set; }

    /// <summary>
    /// Hạng (cho bằng lái xe)
    /// </summary>
    [JsonPropertyName("class")]
    public string? Class { get; set; }

    /// <summary>
    /// Dân tộc
    /// </summary>
    [JsonPropertyName("ethnicity")]
    public string? Ethnicity { get; set; }

    /// <summary>
    /// Tôn giáo
    /// </summary>
    [JsonPropertyName("religion")]
    public string? Religion { get; set; }

    /// <summary>
    /// Đặc điểm nhận dạng
    /// </summary>
    [JsonPropertyName("features")]
    public string? Features { get; set; }

    /// <summary>
    /// Số hộ chiếu (cho hộ chiếu)
    /// </summary>
    [JsonPropertyName("passport_number")]
    public string? PassportNumber { get; set; }

    /// <summary>
    /// Mã quốc gia (cho hộ chiếu)
    /// </summary>
    [JsonPropertyName("country_code")]
    public string? CountryCode { get; set; }

    /// <summary>
    /// Nơi sinh
    /// </summary>
    [JsonPropertyName("place_of_birth")]
    public string? PlaceOfBirth { get; set; }

    /// <summary>
    /// Họ và tên đầy đủ (cho hộ chiếu)
    /// </summary>
    [JsonPropertyName("full_name")]
    public string? FullName { get; set; }

    /// <summary>
    /// Số CMND/CCCD (alias của id, có thể có ở cả hai field)
    /// </summary>
    [JsonPropertyName("id_number")]
    public string? IdNumber { get; set; }
}
