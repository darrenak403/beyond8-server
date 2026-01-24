using System.Net.Http.Headers;
using Amazon;
using Amazon.S3;
using Beyond8.Common.Extensions;
using Beyond8.Common.Utilities;
using Beyond8.Integration.Api.Apis;
using Beyond8.Integration.Application.Consumers.Identity;
using Beyond8.Integration.Application.Dtos.AiIntegration;
using Beyond8.Integration.Application.Dtos.MediaFiles;
using Beyond8.Integration.Application.Dtos.VnptEkyc;
using Beyond8.Integration.Application.Services.Implements;
using Beyond8.Integration.Application.Services.Interfaces;
using Beyond8.Integration.Domain.Repositories.Interfaces;
using Beyond8.Integration.Infrastructure.Configuration;
using Beyond8.Integration.Infrastructure.Configurations;
using Beyond8.Integration.Infrastructure.Data;
using Beyond8.Integration.Infrastructure.ExternalServices;
using Beyond8.Integration.Infrastructure.ExternalServices.Email;
using Beyond8.Integration.Infrastructure.ExternalServices.Hubs.SingalR;
using Beyond8.Integration.Infrastructure.Hubs;
using Beyond8.Integration.Infrastructure.Repositories.Implements;
using FluentValidation;
using Microsoft.Extensions.Options;
using Resend;
using Scalar.AspNetCore;

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

        // Register AI provider configurations
        builder.Services.AddHttpClient();
        builder.Services.Configure<GeminiSettings>(builder.Configuration.GetSection(GeminiSettings.SectionName));
        builder.Services.Configure<BedrockSettings>(builder.Configuration.GetSection(BedrockSettings.SectionName));

        // Register Resend configuration
        builder.Services.Configure<ResendSettings>(
        builder.Configuration.GetSection(ResendSettings.SectionName));

        builder.Services.AddSingleton<IResend>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<ResendSettings>>().Value;
            if (string.IsNullOrWhiteSpace(options.ApiKey))
                throw new InvalidOperationException("ApiKey is missing in ResendSettings");

            return ResendClient.Create(options.ApiKey.Trim());
        });

        // Register VnptEkyc configuration
        builder.Services.Configure<VnptEkycSettings>(builder.Configuration.GetSection(VnptEkycSettings.SectionName));

        builder.Services.AddHttpClient("VnptEkycClient", (serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<VnptEkycSettings>>().Value;

            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);

            if (!string.IsNullOrEmpty(options.AccessToken))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", options.AccessToken);
            }

            if (!string.IsNullOrEmpty(options.TokenId))
            {
                client.DefaultRequestHeaders.Add("Token-id", options.TokenId);
            }

            if (!string.IsNullOrEmpty(options.TokenKey))
            {
                client.DefaultRequestHeaders.Add("Token-key", options.TokenKey);
            }
        });

        // Register repositories
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        builder.Services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = builder.Environment.IsDevelopment();
        });
        builder.Services.AddScoped<INotificationService, SignalRNotificationService>();
        builder.Services.AddScoped<INotificationHistoryService, NotificationHistoryService>();

        // Configure MassTransit with RabbitMQ and register consumers
        builder.AddMassTransitWithRabbitMq(config =>
        {
            // Register consumers from Identity events
            config.AddConsumer<OtpEmailConsumer>();
            config.AddConsumer<InstructorProfileSubmittedConsumer>();
            config.AddConsumer<InstructorApprovalConsumer>();
            config.AddConsumer<InstructorUpdateRequestEmailConsumer>();
        });

        // Register services
        builder.Services.AddScoped<IStorageService, S3Service>();
        builder.Services.AddScoped<IMediaFileService, MediaFileService>();
        builder.Services.AddScoped<IAiUsageService, AiUsageService>();
        builder.Services.AddScoped<IAiPromptService, AiPromptService>();

        // Register AI provider based on configuration
        var aiProvider = builder.Configuration.GetValue<string>("AiProvider") ?? "Gemini";
        if (aiProvider.Equals("Bedrock", StringComparison.OrdinalIgnoreCase))
        {
            builder.Services.AddScoped<IGenerativeAiService, BedrockService>();
        }
        else
        {
            builder.Services.AddScoped<IGenerativeAiService, GeminiService>();
        }

        builder.Services.AddScoped<IUrlContentDownloader, UrlContentDownloader>();
        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.AddScoped<IVnptEkycService, VnptEkycService>();
        builder.Services.AddScoped<IAiService, AiService>();

        // Register validators
        builder.Services.AddValidatorsFromAssemblyContaining<UploadFileRequest>();
        builder.Services.AddValidatorsFromAssemblyContaining<AiUsageRequest>();
        builder.Services.AddValidatorsFromAssemblyContaining<LivenessRequest>();

        return builder;
    }

    public static WebApplication UseApplicationServices(this WebApplication app)
    {
        app.UseCommonService();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.UseHttpsRedirection();

        app.MapHub<AppHub>("/hubs/app")
            .RequireCors("SignalRPolicy");

        app.MapMediaFileApi();
        app.MapAiApi();
        app.MapAiUsageApi();
        app.MapAiPromptApi();
        app.MapVnptEkycApi();
        app.MapNotificationApi();

        return app;
    }
}
