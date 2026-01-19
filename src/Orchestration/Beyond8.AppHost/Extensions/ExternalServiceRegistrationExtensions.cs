using Aspire.Hosting.Yarp;
using Scalar.Aspire;
using Yarp.ReverseProxy.Configuration;

namespace Beyond8.AppHost.Extensions;

public static class ExternalServiceRegistrationExtensions
{
    public static IDistributedApplicationBuilder AddExternalServiceRegistration(this IDistributedApplicationBuilder builder)
    {
        var postgres = builder.AddPostgres("postgres")
            .WithContainerName("PostgresDb")
            .WithImageTag("17-alpine")
            .WithDataVolume()
            .WithPgAdmin(pgAdmin =>
            {
                pgAdmin.WithContainerName("PgAdmin")
                       .WithHostPort(5050);
            });

        var redis = builder.AddRedis("redis-cache")
            .WithImageTag("alpine")
            .WithDataVolume();

        var identityDb = postgres.AddDatabase("identity-db", "Identities");
        var integrationDb = postgres.AddDatabase("integration-db", "Integrations");

        var identityService = builder.AddProject<Projects.Beyond8_Identity_Api>("Identity-Service")
            .WithReference(identityDb)
            .WithReference(redis)
            .WaitFor(postgres)
            .WaitFor(redis);

        var integrationService = builder.AddProject<Projects.Beyond8_Integration_Api>("Integration-Service")
            .WithReference(integrationDb)
            .WaitFor(postgres);

        var apiGateway = builder.AddYarp("api-gateway")
            .WithContainerName("ApiGateway")
            .WithHostPort(8080)
            .WithConfiguration(config =>
            {
                var identityCluster = config.AddProjectCluster(identityService);
                config.AddRoute("/api/v1/auth/{**catch-all}", identityCluster);

                var integrationCluster = config.AddProjectCluster(integrationService);
                config.AddRoute("/api/v1/media/{**catch-all}", integrationCluster);
                config.AddRoute("/api/v1/ai/{**catch-all}", integrationCluster);
            });

        var scalarDocs = builder.AddScalarApiReference("api-docs")
           .WithContainerName("ScalarDocs")
           .WithEndpoint("http", endpoint =>
           {
               endpoint.Port = 8081;
           })
           .WithApiReference(identityService, options => options.AddPreferredSecuritySchemes("Bearer"))
           .WithApiReference(integrationService, options => options.AddPreferredSecuritySchemes("Bearer"));

        return builder;
    }

    private static YarpCluster AddProjectCluster(this IYarpConfigurationBuilder yarp, IResourceBuilder<ProjectResource> resource)
    {
        return yarp.AddCluster(resource).WithHttpClientConfig(new HttpClientConfig
        {
            DangerousAcceptAnyServerCertificate = GetGatewayDangerousAcceptAnyServerCertificate()
        });
    }

    private static YarpRoute AddRoute(this IYarpConfigurationBuilder yarp, string path, IResourceBuilder<ProjectResource> resource)
    {
        var serviceCluster = yarp.AddCluster(resource).WithHttpClientConfig(new HttpClientConfig()
        {
            DangerousAcceptAnyServerCertificate = GetGatewayDangerousAcceptAnyServerCertificate()
        });
        return yarp.AddRoute(path, serviceCluster);
    }

    private static bool GetGatewayDangerousAcceptAnyServerCertificate()
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        return string.Equals(env, "Development", StringComparison.OrdinalIgnoreCase);
    }
}
