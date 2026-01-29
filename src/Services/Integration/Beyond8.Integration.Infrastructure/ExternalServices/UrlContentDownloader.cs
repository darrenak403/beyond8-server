using Beyond8.Integration.Application.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Beyond8.Integration.Infrastructure.ExternalServices;

public class UrlContentDownloader(IHttpClientFactory httpClientFactory, ILogger<UrlContentDownloader> logger) : IUrlContentDownloader
{
    private static readonly Dictionary<string, string> ExtensionMime = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".png"] = "image/png",
        [".webp"] = "image/webp",
        [".gif"] = "image/gif",
        [".pdf"] = "application/pdf"
    };

    public async Task<(byte[]? Data, string? MimeType)> DownloadAsync(string url, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url)) return (null, null);

        try
        {
            using var client = httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(30);
            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Download failed {Url}: {StatusCode}", url, response.StatusCode);
                return (null, null);
            }

            var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            var mime = response.Content.Headers.ContentType?.MediaType?.Trim();
            if (string.IsNullOrEmpty(mime))
                mime = InferMimeFromPath(url);

            return (data, mime ?? "application/octet-stream");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Download failed {Url}", url);
            return (null, null);
        }
    }

    private static string? InferMimeFromPath(string url)
    {
        var path = url.AsSpan();
        var q = path.IndexOf('?');
        if (q >= 0) path = path[..q];
        var last = path.LastIndexOf('.');
        if (last < 0) return null;
        var ext = path[last..].ToString();
        return ExtensionMime.TryGetValue(ext, out var m) ? m : null;
    }
}
