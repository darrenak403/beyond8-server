using Beyond8.Analytic.Api.Apis;
using Beyond8.Analytic.Application.Consumers.Assessment;
using Beyond8.Analytic.Application.Consumers.Catalog;
using Beyond8.Analytic.Application.Consumers.Learning;
using Beyond8.Analytic.Application.Consumers.Sale;
using Beyond8.Analytic.Application.Services.Implements;
using Beyond8.Analytic.Application.Services.Interfaces;
using Beyond8.Analytic.Domain.Repositories.Interfaces;
using Beyond8.Analytic.Infrastructure.Data;
using Beyond8.Analytic.Infrastructure.Repositories.Implements;
using Beyond8.Common.Extensions;
using Beyond8.Common.Utilities;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Beyond8.Analytic.Api.Bootstrapping;

public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddOpenApi();
        builder.AddCommonExtensions();
        builder.AddPostgresDatabase<AnalyticDbContext>(Const.AnalyticServiceDatabase, options =>
            options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));
        builder.AddServiceRedis(nameof(Analytic), connectionName: Const.Redis);

        builder.AddMassTransitWithRabbitMq(config =>
        {
            // Learning events
            config.AddConsumer<CourseEnrollmentCountChangedEventConsumer>();
            config.AddConsumer<CourseCompletedEventConsumer>();
            config.AddConsumer<CourseRatingUpdatedEventConsumer>();
            // Catalog events
            config.AddConsumer<CoursePublishedEventConsumer>();
            config.AddConsumer<CourseUpdatedMetadataEventConsumer>();
            // Sale events
            config.AddConsumer<OrderItemCompletedEventConsumer>();
            // Assessment events
            config.AddConsumer<QuizAttemptCompletedEventConsumer>();
        }, queueNamePrefix: "analytic");

        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<ICourseStatsService, CourseStatsService>();
        builder.Services.AddScoped<IInstructorRevenueService, InstructorRevenueService>();
        builder.Services.AddScoped<ISystemOverviewService, SystemOverviewService>();
        builder.Services.AddScoped<ILessonPerformanceService, LessonPerformanceService>();

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
        app.MapSystemOverviewApi();
        app.MapCourseStatsApi();
        app.MapInstructorAnalyticsApi();
        app.MapLessonPerformanceApi();

        return app;
    }
}
