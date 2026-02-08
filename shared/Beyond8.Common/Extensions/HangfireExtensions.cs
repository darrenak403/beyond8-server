using Hangfire;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Beyond8.Common.Utilities;

namespace Beyond8.Common.Extensions;

public static class HangfireExtensions
{
    private const string DefaultConnectionName = Const.HangfireDatabase;

    public static IHostApplicationBuilder AddHangfire(
        this IHostApplicationBuilder builder,
        string connectionName = DefaultConnectionName,
        Action<IGlobalConfiguration>? configure = null)
    {
        var connectionString = builder.Configuration.GetConnectionString(connectionName);

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{connectionName}' not found. Ensure the Hangfire database is configured (e.g. via Aspire AppHost WithReference).");
        }

        builder.Services.AddHangfire(config =>
        {
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString));

            configure?.Invoke(config);
        });

        var workerCount = builder.Configuration.GetValue<int>("Hangfire:WorkerCount", Environment.ProcessorCount * 2);
        builder.Services.AddHangfireServer(options => options.WorkerCount = workerCount);

        return builder;
    }

    public static WebApplication UseHangfireDashboard(
        this WebApplication app,
        string path = "/hangfire",
        bool allowAnonymousInDevelopment = false)
    {
        var dashboardPath = app.Configuration.GetValue<string>("Hangfire:DashboardPath") ?? path;

        var options = new DashboardOptions
        {
            Authorization = new[]
            {
                new HangfireDashboardAuthorizationFilter(allowAnonymousInDevelopment && app.Environment.IsDevelopment())
            }
        };

        app.UseHangfireDashboard(dashboardPath, options);

        return app;
    }
}

internal sealed class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    private readonly bool _allowAnonymous;

    public HangfireDashboardAuthorizationFilter(bool allowAnonymous = false)
    {
        _allowAnonymous = allowAnonymous;
    }

    public bool Authorize(DashboardContext context)
    {
        if (_allowAnonymous)
            return true;

        var httpContext = context.GetHttpContext();
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
            return false;

        return httpContext.User.IsInRole(Role.Admin);
    }
}
