using System.Net.Http.Headers;
using Amazon;
using Amazon.S3;
using Beyond8.Common.Extensions;
using Beyond8.Common.Utilities;
using Beyond8.Integration.Api.Apis;
using Beyond8.Integration.Application.Clients;
using Beyond8.Integration.Application.Consumers.Assessment;
using Beyond8.Integration.Application.Consumers.Catalog;
using Beyond8.Integration.Application.Consumers.Identity;
using Beyond8.Integration.Application.Dtos.MediaFiles;
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
using Polly;
using Polly.Extensions.Http;
using Resend;
using Scalar.AspNetCore;

namespace Beyond8.Integration.Api.Bootstrapping
{
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

            var aiProvider = builder.Configuration.GetValue<string>("AiProvider") ?? "Gemini";
            if (aiProvider.Equals("Bedrock", StringComparison.OrdinalIgnoreCase))
            {
                builder.Services.AddScoped<IGenerativeAiService, BedrockService>();
            }
            else
            {
                builder.Services.AddScoped<IGenerativeAiService, GeminiService>();
            }

            // Register Resend configuration
            builder.Services.Configure<ResendSettings>(
            builder.Configuration.GetSection(ResendSettings.SectionName));

            builder.Services.AddSingleton<IResend>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<ResendSettings>>().Value;
                if (string.IsNullOrWhiteSpace(options.ApiKey))
                    throw new InvalidOperationException("ApiKey is missing in Resend Settings");

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

            // Configure MassTransit with RabbitMQ and register consumers
            builder.AddMassTransitWithRabbitMq(config =>
            {
                // Register consumers from Identity events
                config.AddConsumer<OtpEmailConsumer>();
                config.AddConsumer<InstructorProfileSubmittedConsumer>();
                config.AddConsumer<InstructorApprovalConsumer>();
                config.AddConsumer<InstructorUpdateRequestEmailConsumer>();

                // Register consumers from Catalog events
                config.AddConsumer<CourseRejectedEventConsumer>();
                config.AddConsumer<CourseApprovedEventConsumer>();

                // Register consumers from Transcoding events
                config.AddConsumer<TranscodingVideoSuccessEventConsumer>();

                // Register consumers from Assessment events
                config.AddConsumer<AssignmentSubmittedConsumer>();
            });

            // Configure Qdrant - Use Aspire Qdrant Client
            builder.AddQdrantClient(Const.Qdrant);

            // Configure Qdrant Settings (for vector dimension config)
            builder.Services.Configure<QdrantSettings>(builder.Configuration.GetSection(QdrantSettings.SectionName));

            // Hugging Face Embedding Service
            builder.Services.Configure<HuggingFaceSettings>(builder.Configuration.GetSection(HuggingFaceSettings.SectionName));

            // Register clients
            builder.AddClientServices();

            // Register services
            builder.Services.AddScoped<INotificationService, SignalRNotificationService>();
            builder.Services.AddScoped<INotificationHistoryService, NotificationHistoryService>();
            builder.Services.AddScoped<IStorageService, S3Service>();
            builder.Services.AddScoped<IMediaFileService, MediaFileService>();
            builder.Services.AddScoped<IAiUsageService, AiUsageService>();
            builder.Services.AddScoped<IAiPromptService, AiPromptService>();
            builder.Services.AddScoped<IUrlContentDownloader, UrlContentDownloader>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IVnptEkycService, VnptEkycService>();
            builder.Services.AddScoped<IAiService, AiService>();
            builder.Services.AddScoped<IEmbeddingService, EmbeddingService>();
            builder.Services.AddScoped<IPdfChunkService, PdfChunkService>();
            builder.Services.AddScoped<IVectorEmbeddingService, VectorEmbeddingService>();

            // Register validators
            builder.Services.AddValidatorsFromAssemblyContaining<UploadFileRequest>();

            return builder;
        }

        public static IHostApplicationBuilder AddClientServices(this IHostApplicationBuilder builder)
        {
            builder.Services.AddHttpContextAccessor();

            var identityBaseUrl = builder.Configuration["Clients:Identity:BaseUrl"]
                                  ?? throw new ArgumentNullException("Identity URL missing");

            builder.Services.AddHttpClient<IIdentityClient, IdentityClient>(client =>
            {
                client.BaseAddress = new Uri(identityBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddPolicyHandler(GetResiliencePolicy());

            return builder;
        }

        private static IAsyncPolicy<HttpResponseMessage> GetResiliencePolicy()
        {
            var jitterer = new Random();

            var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt =>
                        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                        + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000))
                );

            var circuitBreakerPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(30)
                );

            return Policy.WrapAsync(retryPolicy, circuitBreakerPolicy);
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
                .RequireCors(app.Environment.IsDevelopment() ? "AllowDevelopmentClients" : "AllowProductionClients");

            app.MapMediaFileApi();
            app.MapAiApi();
            app.MapAiUsageApi();
            app.MapAiPromptApi();
            app.MapVnptEkycApi();
            app.MapNotificationApi();

            return app;
        }
    }
}
