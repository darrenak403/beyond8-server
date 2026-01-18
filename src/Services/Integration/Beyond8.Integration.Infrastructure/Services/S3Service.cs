using Amazon.S3;
using Amazon.S3.Model;
using Beyond8.Integration.Application.Services;
using Beyond8.Integration.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Beyond8.Integration.Infrastructure.Services;

public class S3Service : IStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<S3Service> _logger;
    private readonly S3Settings _s3Settings;

    public S3Service(IAmazonS3 s3Client, ILogger<S3Service> logger, IOptions<S3Settings> s3Settings)
    {
        _s3Client = s3Client;
        _logger = logger;
        _s3Settings = s3Settings.Value;
    }

    public async Task<string> GeneratePresignedUrlAsync(string fileKey, string contentType)
    {
        try
        {
            _logger.LogInformation("Generating presigned URL for upload: Bucket={BucketName}, Key={FileKey}, ContentType={ContentType}",
                _s3Settings.BucketName, fileKey, contentType);

            var request = new GetPreSignedUrlRequest
            {
                BucketName = _s3Settings.BucketName,
                Key = fileKey,
                Verb = HttpVerb.PUT,
                Expires = DateTime.UtcNow.AddMinutes(15),
                ContentType = contentType
            };

            var url = await _s3Client.GetPreSignedURLAsync(request);
            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating presigned URL: Key={FileKey}", fileKey);
            throw;
        }
    }

    public async Task DeleteFileAsync(string fileKey)
    {
        try
        {
            _logger.LogInformation("Deleting file from S3: Bucket={BucketName}, Key={FileKey}", _s3Settings.BucketName, fileKey);

            var request = new DeleteObjectRequest
            {
                BucketName = _s3Settings.BucketName,
                Key = fileKey
            };

            await _s3Client.DeleteObjectAsync(request);

            _logger.LogInformation("File deleted successfully from S3: Key={FileKey}", fileKey);
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "AWS S3 error while deleting file: Key={FileKey}, ErrorCode={ErrorCode}",
                fileKey, ex.ErrorCode);
            throw new Exception($"AWS S3 error: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file from S3: Key={FileKey}", fileKey);
            throw;
        }
    }

    public string GetFilePath(string fileKey)
    {
        try
        {
            string url;

            if (!string.IsNullOrEmpty(_s3Settings.CloudFrontUrl))
            {
                url = $"{_s3Settings.CloudFrontUrl.TrimEnd('/')}/{fileKey}";
                _logger.LogDebug("Generated CloudFront URL: {Url} for Key={FileKey}", url, fileKey);
            }
            else
            {
                url = $"https://{_s3Settings.BucketName}.s3.{_s3Settings.Region}.amazonaws.com/{fileKey}";
                _logger.LogDebug("Generated S3 URL: {Url} for Key={FileKey}", url, fileKey);
            }

            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating file path for Key={FileKey}", fileKey);
            throw;
        }
    }

    public string ExtractKeyFromUrl(string url)
    {
        try
        {
            _logger.LogDebug("Extracting key from URL: {Url}", url);

            var uri = new Uri(url);
            var path = uri.AbsolutePath;

            var key = path.TrimStart('/');

            if (key.StartsWith(_s3Settings.BucketName + "/"))
            {
                key = key.Substring(_s3Settings.BucketName.Length + 1);
            }

            _logger.LogDebug("Extracted key: {Key} from URL: {Url}", key, url);
            return key;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting key from URL: {Url}", url);
            throw;
        }
    }
}
