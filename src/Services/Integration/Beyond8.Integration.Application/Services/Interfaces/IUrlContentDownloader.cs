namespace Beyond8.Integration.Application.Services.Interfaces
{
    public interface IUrlContentDownloader
    {
        Task<(byte[]? Data, string? MimeType)> DownloadAsync(string url, CancellationToken cancellationToken = default);
    }
}
