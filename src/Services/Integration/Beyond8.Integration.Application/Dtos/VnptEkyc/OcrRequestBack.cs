using System.Text.Json.Serialization;

namespace Beyond8.Integration.Application.Dtos.VnptEkyc
{
    /// <summary>
    /// Request cho OCR mặt sau giấy tờ
    /// </summary>
    public class OcrRequestBack
    {
        /// <summary>
        /// Hash của ảnh mặt sau (từ upload endpoint)
        /// </summary>
        [JsonPropertyName("img_back")]
        public string ImgBack { get; set; } = string.Empty;

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
        /// Token để xác thực request
        /// </summary>
        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;
    }
}
