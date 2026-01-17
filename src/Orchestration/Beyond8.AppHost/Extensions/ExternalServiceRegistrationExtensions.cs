using System;
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

        var apiGateway = builder.AddYarp("api-gateway")
            .WithContainerName("ApiGateway")
            .WithHostPort(8080)
            .WithConfiguration(config =>
            {
            });

        var scalarDocs = builder.AddScalarApiReference("scalar-docs")
            .WithContainerName("ScalarDocs");

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
