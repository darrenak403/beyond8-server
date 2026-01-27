using Beyond8.Common.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json.Serialization;

namespace Beyond8.Common.Extensions;

public static class CommonExtensions
{
    public static IHostApplicationBuilder AddCommonExtensions(this IHostApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
                    {
                        options.AddPolicy("AllowDevelopmentClients", builder =>
                        {
                            builder.WithOrigins(
                                       "http://localhost:3000",
                                       "http://localhost:5173",
                                       "http://api-gateway.beyond8.dev",
                                       "https://api-gateway.beyond8.dev",
                                       "http://api-gateway-beyond8.dev.localhost:8080",
                                       "https://api-gateway-beyond8.dev.localhost:8080"
                                   )
                                   .AllowAnyMethod()
                                   .AllowAnyHeader()
                                   .AllowCredentials();
                        });

                        options.AddPolicy("AllowProductionClients", builder =>
                        {
                            builder.WithOrigins(
                                       "https://beyond8.io.vn",
                                       "https://www.beyond8.io.vn",
                                       "https://app.beyond8.io.vn",
                                       "https://admin.beyond8.io.vn"
                                   )
                                   .AllowAnyMethod()
                                   .AllowAnyHeader()
                                   .AllowCredentials();
                        });
                    });

        builder.Services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter(policyName: "Fixed", options =>
            {
                options.PermitLimit = 100;
                options.Window = TimeSpan.FromMinutes(1);
                options.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
                options.QueueLimit = 0;
            });

            options.AddTokenBucketLimiter(policyName: "AiFixedLimit", options =>
            {
                options.TokenLimit = 10;
                options.ReplenishmentPeriod = TimeSpan.FromSeconds(60);
                options.TokensPerPeriod = 10;
                options.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
                options.QueueLimit = 0;
            });
        });

        builder.Services.Configure<RateLimiterOptions>(options =>
        {
            options.RejectionStatusCode = 429;
        });

        builder.Services.Configure<JsonOptions>(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        builder.AddJwtAuthentication();
        builder.AddDocumentGlobalAuthentication();

        return builder;
    }

    public static WebApplication UseCommonService(this WebApplication app)
    {

        // CORS must be called before other middleware
        if (app.Environment.IsDevelopment())
        {
            app.UseCors("AllowDevelopmentClients");
        }
        else
        {
            app.UseCors("AllowProductionClients");
        }

        // Exception handling after CORS
        app.UseMiddleware<GlobalExceptionsMiddleware>();

        // Authentication and Authorization
        app.UseAuthentication();
        app.UseAuthorization();

        // Rate limiting last
        app.UseRateLimiter();

        return app;
    }
}
