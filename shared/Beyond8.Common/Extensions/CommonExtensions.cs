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
                            builder.WithOrigins("http://localhost:3000", "http://localhost:5173")
                                   .AllowAnyMethod()
                                   .AllowAnyHeader();
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
        });

        builder.Services.Configure<RateLimiterOptions>(options =>
        {
            options.RejectionStatusCode = 429;
        });

        // builder.Services.AddMassTransit(config =>
        // {
        //     config.AddConsumers(Assembly.GetEntryAssembly());

        //     config.UsingRabbitMq((context, cfg) =>
        //     {
        //         var rabbitMqConnectionString = builder.Configuration.GetConnectionString(Const.RabbitMQ)
        //             ?? throw new ArgumentNullException(nameof(builder.Configuration), "RabbitMq connection string is empty");

        //         cfg.Host(new Uri(rabbitMqConnectionString));

        //         cfg.ConfigureEndpoints(context);

        //         cfg.UseMessageRetry(retry =>
        //         {
        //             retry.Exponential(
        //                 retryLimit: 5,
        //                 minInterval: TimeSpan.FromSeconds(2),
        //                 maxInterval: TimeSpan.FromSeconds(30),
        //                 intervalDelta: TimeSpan.FromSeconds(5)
        //             );
        //         });
        //     });
        // });

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
