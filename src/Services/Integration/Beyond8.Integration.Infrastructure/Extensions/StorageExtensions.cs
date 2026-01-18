using Amazon.S3;
using Beyond8.Integration.Application.Services;
using Beyond8.Integration.Infrastructure.Configuration;
using Beyond8.Integration.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Beyond8.Integration.Infrastructure.Extensions;

public static class StorageExtensions
{
    public static IHostApplicationBuilder AddS3Storage(this IHostApplicationBuilder builder)
    {
        var s3Settings = builder.Configuration.GetSection(S3Settings.SectionName).Get<S3Settings>()
            ?? throw new InvalidOperationException($"Configuration section '{S3Settings.SectionName}' not found");

        builder.Services.Configure<S3Settings>(builder.Configuration.GetSection(S3Settings.SectionName));

        builder.Services.AddSingleton<IAmazonS3>(sp =>
        {
            var config = new AmazonS3Config
            {
                RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(s3Settings.Region)
            };

            return new AmazonS3Client(s3Settings.AccessKey, s3Settings.SecretKey, config);
        });

        builder.Services.AddScoped<IStorageService, S3Service>();

        return builder;
    }

    public static IServiceCollection AddS3Storage(this IServiceCollection services, IConfiguration configuration)
    {
        var s3Settings = configuration.GetSection(S3Settings.SectionName).Get<S3Settings>()
            ?? throw new InvalidOperationException($"Configuration section '{S3Settings.SectionName}' not found");

        services.Configure<S3Settings>(configuration.GetSection(S3Settings.SectionName));

        services.AddSingleton<IAmazonS3>(sp =>
        {
            var config = new AmazonS3Config
            {
                RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(s3Settings.Region)
            };

            return new AmazonS3Client(s3Settings.AccessKey, s3Settings.SecretKey, config);
        });

        services.AddScoped<IStorageService, S3Service>();

        return services;
    }
}
