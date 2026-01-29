namespace Beyond8.Integration.Application.Services.Interfaces;

/// <summary>Download nội dung từ URL (ảnh, PDF) dùng cho Gemini multimodal.</summary>
public interface IUrlContentDownloader
{
    /// <summary>Tải URL trả về (Data, MimeType). MimeType lấy từ Content-Type hoặc suy ra từ đuôi file.</summary>
    Task<(byte[]? Data, string? MimeType)> DownloadAsync(string url, CancellationToken cancellationToken = default);
}
