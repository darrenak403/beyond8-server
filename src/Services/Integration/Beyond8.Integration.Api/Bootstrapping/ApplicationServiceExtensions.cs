using Amazon;
using Amazon.S3;
using Beyond8.Common.Extensions;
using Beyond8.Common.Utilities;
using Beyond8.Integration.Api.Apis;
using Beyond8.Integration.Application.Dtos.AiIntegration;
using Beyond8.Integration.Application.Dtos.MediaFiles;
using Beyond8.Integration.Application.Services.Implements;
using Beyond8.Integration.Application.Services.Interfaces;
using Beyond8.Integration.Domain.Repositories.Interfaces;
using Beyond8.Integration.Infrastructure.Configuration;
using Beyond8.Integration.Infrastructure.Data;
using Beyond8.Integration.Infrastructure.ExternalServices;
using Beyond8.Integration.Infrastructure.Repositories.Implements;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace Beyond8.Integration.Api.Bootstrapping;

public static class Bootstrapper
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddOpenApi();

        builder.AddCommonExtensions();

        builder.AddPostgresDatabase<IntegrationDbContext>(Const.IntegrationServiceDatabase);

        builder.AddServiceRedis(nameof(Integration), connectionName: Const.Redis);

        // Configure AWS S3
        builder.Services.Configure<S3Settings>(builder.Configuration.GetSection(S3Settings.SectionName));

        builder.Services.AddSingleton<IAmazonS3>(sp =>
        {
            var s3Settings = sp.GetRequiredService<IOptions<S3Settings>>().Value;
            var config = new AmazonS3Config
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName(s3Settings.Region),
                ForcePathStyle = true
            };

            return new AmazonS3Client(s3Settings.AccessKey, s3Settings.SecretKey, config);
        });

        // Register Gemini configuration
        builder.Services.AddHttpClient();
        builder.Services.Configure<GeminiConfiguration>(builder.Configuration.GetSection(GeminiConfiguration.SectionName));

        // Register repositories
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register services
        builder.Services.AddScoped<IStorageService, S3Service>();
        builder.Services.AddScoped<IMediaFileService, MediaFileService>();
        builder.Services.AddScoped<IAiUsageService, AiUsageService>();
        builder.Services.AddScoped<IAiPromptService, AiPromptService>();
        builder.Services.AddScoped<IGeminiService, GeminiService>();

        // Register validators
        builder.Services.AddValidatorsFromAssemblyContaining<UploadFileRequest>();
        builder.Services.AddValidatorsFromAssemblyContaining<AiUsageRequest>();

        return builder;
    }

    public static WebApplication UseApplicationServices(this WebApplication app)
    {
        app.UseCommonService();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.MapMediaFileApi();
        app.MapAiUsageApi();
        app.MapAiPromptApi();

        return app;
    }
}
