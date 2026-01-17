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

        var identityDb = postgres.AddDatabase("identity-db", "Identities");

        var identityService = builder.AddProject<Projects.Beyond8_Identity_Api>("Identity-Service")
            .WithReference(identityDb)
            .WaitFor(postgres);

        var apiGateway = builder.AddYarp("api-gateway")
            .WithContainerName("ApiGateway")
            .WithHostPort(8080)
            .WithConfiguration(config =>
            {

            });

        var scalarDocs = builder.AddScalarApiReference("api-docs")
           .WithContainerName("ScalarDocs")
           .WithEndpoint("http", endpoint =>
           {
               endpoint.Port = 8081;
           })
           .WithApiReference(identityService);

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
