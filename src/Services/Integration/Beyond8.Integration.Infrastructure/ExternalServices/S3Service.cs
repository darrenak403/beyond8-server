using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Beyond8.Integration.Application.Services.Interfaces;
using Beyond8.Integration.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Beyond8.Integration.Infrastructure.ExternalServices;

public class S3Service(IOptions<S3Settings> s3Settings, ILogger<S3Service> logger, IAmazonS3 s3Client) : IStorageService
{
    private readonly S3Settings _s3Settings = s3Settings.Value;
    private readonly ILogger<S3Service> _logger = logger;
    private readonly IAmazonS3 _s3Client = s3Client;

    public string GeneratePresignedUploadUrl(string fileKey, string contentType, int expirationMinutes = 15)
    {
        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _s3Settings.BucketName,
                Key = fileKey,
                Verb = HttpVerb.PUT,
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                ContentType = contentType
            };

            var presignedUrl = _s3Client.GetPreSignedURL(request);
            _logger.LogInformation("Generated presigned upload URL for key: {FileKey}", fileKey);
            return presignedUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating presigned upload URL for key: {FileKey}", fileKey);
            throw;
        }
    }

    public string GeneratePresignedDownloadUrl(string fileKey, int expirationMinutes = 60)
    {
        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _s3Settings.BucketName,
                Key = fileKey,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes)
            };

            var presignedUrl = _s3Client.GetPreSignedURL(request);
            _logger.LogInformation("Generated presigned download URL for key: {FileKey}", fileKey);
            return presignedUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating presigned download URL for key: {FileKey}", fileKey);
            throw;
        }
    }

    public string GeneratePresignedDownloadUrl(
        string fileKey,
        string fileName,
        string disposition,
        int expirationMinutes = 15)
    {
        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _s3Settings.BucketName,
                Key = fileKey,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                ResponseHeaderOverrides = new ResponseHeaderOverrides
                {
                    ContentDisposition = $"{disposition}; filename=\"{fileName}\""
                }
            };

            var presignedUrl = _s3Client.GetPreSignedURL(request);
            _logger.LogInformation(
                "Generated presigned download URL with disposition for key: {FileKey}",
                fileKey);
            return presignedUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error generating presigned download URL for key: {FileKey}",
                fileKey);
            throw;
        }
    }

    public async Task<bool> FileExistsAsync(string fileKey)
    {
        try
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = _s3Settings.BucketName,
                Key = fileKey
            };

            await _s3Client.GetObjectMetadataAsync(request);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking file existence for key: {FileKey}", fileKey);
            throw;
        }
    }

    public async Task DeleteFileAsync(string fileKey)
    {
        try
        {
            var request = new DeleteObjectRequest
            {
                BucketName = _s3Settings.BucketName,
                Key = fileKey
            };

            await _s3Client.DeleteObjectAsync(request);
            _logger.LogInformation("Deleted file with key: {FileKey}", fileKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file with key: {FileKey}", fileKey);
            throw;
        }
    }

    public async Task<(byte[]? Data, string? ContentType)> GetObjectAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileKey)) return (null, null);

        try
        {
            var request = new GetObjectRequest { BucketName = _s3Settings.BucketName, Key = fileKey };
            using var response = await _s3Client.GetObjectAsync(request, cancellationToken);
            await using var ms = new MemoryStream();
            await response.ResponseStream.CopyToAsync(ms, cancellationToken);
            var data = ms.ToArray();
            var contentType = response.Headers?.ContentType?.Trim();
            if (string.IsNullOrWhiteSpace(contentType))
                contentType = InferMimeFromKey(fileKey);
            return (data, contentType ?? "application/octet-stream");
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogDebug("S3 key not found: {FileKey}", fileKey);
            return (null, null);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting S3 object: {FileKey}", fileKey);
            return (null, null);
        }
    }

    public string GetFilePath(string fileKey)
    {
        if (!string.IsNullOrWhiteSpace(_s3Settings.CloudFrontUrl))
        {
            return $"{_s3Settings.CloudFrontUrl.TrimEnd('/')}/{fileKey}";
        }

        return $"https://{_s3Settings.BucketName}.s3.{_s3Settings.Region}.amazonaws.com/{fileKey}";
    }

    public string ExtractKeyFromUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return string.Empty;

        try
        {
            var uri = new Uri(url);

            // CloudFront: so sánh host (CloudFrontUrl có thể là "d30z0qh7rhzgt8.cloudfront.net" hoặc full URL)
            var cfHost = _s3Settings.CloudFrontUrl;
            if (!string.IsNullOrWhiteSpace(cfHost) && cfHost.Contains("://", StringComparison.Ordinal))
            {
                if (Uri.TryCreate(cfHost, UriKind.Absolute, out var cu))
                    cfHost = cu.Host;
            }
            if (!string.IsNullOrWhiteSpace(cfHost) && uri.Host.Equals(cfHost, StringComparison.OrdinalIgnoreCase))
                return uri.AbsolutePath.TrimStart('/');

            // S3 URL (bucket.s3.region.amazonaws.com hoặc s3.region.amazonaws.com)
            if (uri.Host.Contains("s3", StringComparison.OrdinalIgnoreCase))
                return uri.AbsolutePath.TrimStart('/');

            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting key from URL: {Url}", url);
            return string.Empty;
        }
    }

    public string BuildFileKey(string folder, Guid userId, string fileName, string? subFolder = null)
    {
        var sanitizedFileName = SanitizeFileName(fileName);
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var uniqueFileName = $"{timestamp}_{sanitizedFileName}";

        if (!string.IsNullOrWhiteSpace(subFolder))
        {
            return $"{folder}/{userId}/{subFolder}/{uniqueFileName}";
        }

        return $"{folder}/{userId}/{uniqueFileName}";
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        return sanitized;
    }

    private static readonly Dictionary<string, string> KeyExtensionMime = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".png"] = "image/png",
        [".webp"] = "image/webp",
        [".gif"] = "image/gif",
        [".pdf"] = "application/pdf"
    };

    private static string? InferMimeFromKey(string fileKey)
    {
        var last = fileKey.AsSpan().LastIndexOf('.');
        if (last < 0) return null;
        var ext = fileKey.AsSpan(last..).ToString();
        return KeyExtensionMime.TryGetValue(ext, out var m) ? m : null;
    }
}
