namespace Beyond8.Integration.Application.Services.Interfaces;

public interface IStorageService
{
    Task<string> GeneratePresignedUrlAsync(string fileKey, string contentType);

    Task DeleteFileAsync(string fileKey);

    string GetFilePath(string fileKey);

    string ExtractKeyFromUrl(string url);
}
