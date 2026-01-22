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
                                       "https://api-gateway.beyond8.dev"
                                   )
                                   .AllowAnyMethod()
                                   .AllowAnyHeader()
                                   .AllowCredentials();
                        });

                        options.AddPolicy("SignalRPolicy", builder =>
                        {
                            builder.WithOrigins(
                                       "http://localhost:3000",
                                       "http://localhost:5173",
                                       "http://api-gateway.beyond8.dev",
                                       "https://api-gateway.beyond8.dev"
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
        app.UseCors("AllowDevelopmentClients");
        app.UseMiddleware<GlobalExceptionsMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseRateLimiter();

        return app;
    }
}
