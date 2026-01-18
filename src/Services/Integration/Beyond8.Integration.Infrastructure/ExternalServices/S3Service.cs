using System;
using Amazon.S3;
using Beyond8.Integration.Application.Services.Interfaces;
using Beyond8.Integration.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace Beyond8.Integration.Infrastructure.ExternalServices;

public class S3Service(IOptions<S3Settings> s3Settings, ILogger<S3Service> logger, IAmazonS3 s3Client) : IStorageService
{
    private readonly S3Settings _s3Settings = s3Settings.Value;
    private readonly ILogger<S3Service> _logger = logger;
    private readonly IAmazonS3 _s3Client = s3Client;

    public Task DeleteFileAsync(string fileKey)
    {
        throw new NotImplementedException();
    }

    public string ExtractKeyFromUrl(string url)
    {
        throw new NotImplementedException();
    }

    public Task<string> GeneratePresignedUrlAsync(string fileKey, string contentType)
    {
        throw new NotImplementedException();
    }

    public string GetFilePath(string fileKey)
    {
        throw new NotImplementedException();
    }
}
