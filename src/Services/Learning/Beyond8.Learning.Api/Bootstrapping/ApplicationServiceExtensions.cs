using Beyond8.Common.Extensions;
using Beyond8.Common.Utilities;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Beyond8.Learning.Api.Apis;
using Beyond8.Learning.Application.Clients.Catalog;
using Beyond8.Learning.Application.Consumers.Assessment;
using Beyond8.Learning.Application.Services.Interfaces;
using Beyond8.Learning.Application.Services.Implements;
using Beyond8.Learning.Application.Dtos.Enrollments;
using Beyond8.Learning.Domain.Repositories.Interfaces;
using Beyond8.Learning.Infrastructure.Data;
using Beyond8.Learning.Infrastructure.Repositories.Implements;
using FluentValidation;
using Polly;
using Polly.Extensions.Http;

namespace Beyond8.Learning.Api.Bootstrapping;

public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddOpenApi();
        builder.AddCommonExtensions();
        builder.AddPostgresDatabase<LearningDbContext>(Const.LearningServiceDatabase, options =>
            options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));
        builder.AddServiceRedis(nameof(Learning), connectionName: Const.Redis);

        builder.AddMassTransitWithRabbitMq(config =>
        {
            config.AddConsumer<QuizAttemptCompletedEventConsumer>();
            config.AddConsumer<AssignmentSubmittedEventConsumer>();
            config.AddConsumer<AiGradingCompletedEventConsumer>();
            config.AddConsumer<AssignmentGradedEventConsumer>();
        }, queueNamePrefix: "learning");

        builder.Services.AddValidatorsFromAssemblyContaining<EnrollFreeRequest>();

        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
        builder.Services.AddScoped<IProgressService, ProgressService>();
        builder.Services.AddScoped<ICertificateService, CertificateService>();
        builder.Services.AddScoped<ICourseReviewService, CourseReviewService>();

        builder.AddCatalogClient();

        return builder;
    }

    private static IHostApplicationBuilder AddCatalogClient(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();

        var catalogBaseUrl = builder.Configuration["Clients:Catalog:BaseUrl"]
            ?? throw new InvalidOperationException("Clients:Catalog:BaseUrl is required for Learning service.");

        builder.Services.AddHttpClient<ICatalogClient, CatalogClient>(client =>
        {
            client.BaseAddress = new Uri(catalogBaseUrl);
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
        }
        app.UseHttpsRedirection();
        app.MapEnrollmentApi();
        app.MapCertificateApi();
        app.MapCourseReviewApi();

        return app;
    }
}
