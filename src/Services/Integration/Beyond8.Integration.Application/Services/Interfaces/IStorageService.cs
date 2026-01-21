namespace Beyond8.Integration.Application.Services.Interfaces;

public interface IStorageService
{
    string GeneratePresignedUploadUrl(string fileKey, string contentType, int expirationMinutes = 15);
    string GeneratePresignedDownloadUrl(string fileKey, int expirationMinutes = 60);
    Task<bool> FileExistsAsync(string fileKey);
    Task DeleteFileAsync(string fileKey);
    string GetFilePath(string fileKey);
    string ExtractKeyFromUrl(string url);

    /// <summary>Lấy nội dung file từ S3 theo key. Trả về (Data, ContentType). Dùng khi có S3 key hoặc key rút từ URL CloudFront/S3.</summary>
    Task<(byte[]? Data, string? ContentType)> GetObjectAsync(string fileKey, CancellationToken cancellationToken = default);

    string BuildFileKey(string folder, Guid userId, string fileName, string? subFolder = null);
}
