using System.Text.Json.Serialization;

namespace Beyond8.Integration.Application.Dtos.VnptEkyc
{
    /// <summary>
    /// Request cho OCR mặt trước giấy tờ
    /// </summary>
    public class OcrRequestFront
    {
        /// <summary>
        /// Hash của ảnh mặt trước (từ upload endpoint)
        /// </summary>
        [JsonPropertyName("img_front")]
        public string ImgFront { get; set; } = string.Empty;

        /// <summary>
        /// Session ID của client
        /// </summary>
        [JsonPropertyName("client_session")]
        public string ClientSession { get; set; } = string.Empty;

        /// <summary>
        /// Loại giấy tờ (1 = CMND/CCCD, 2 = Hộ chiếu, ...)
        /// </summary>
        [JsonPropertyName("type")]
        public int Type { get; set; } = 1;

        /// <summary>
        /// Có validate mã bưu điện không (optional)
        /// </summary>
        [JsonPropertyName("validate_postcode")]
        public bool? ValidatePostcode { get; set; }

        /// <summary>
        /// Token để xác thực request
        /// </summary>
        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;
    }
}
