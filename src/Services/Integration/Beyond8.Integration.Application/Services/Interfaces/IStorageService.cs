namespace Beyond8.Integration.Application.Services.Interfaces;

public interface IStorageService
{
    string GeneratePresignedUploadUrl(string fileKey, string contentType, int expirationMinutes = 15);
    string GeneratePresignedDownloadUrl(string fileKey, int expirationMinutes = 60);
    Task<bool> FileExistsAsync(string fileKey);
    Task DeleteFileAsync(string fileKey);
    string GetFilePath(string fileKey);
    string ExtractKeyFromUrl(string url);
    Task<(byte[]? Data, string? ContentType)> GetObjectAsync(string fileKey, CancellationToken cancellationToken = default);
    string BuildFileKey(string folder, Guid userId, string fileName, string? subFolder = null);
}
