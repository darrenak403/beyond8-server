using Aspire.Hosting.Yarp;
using Scalar.Aspire;
using Yarp.ReverseProxy.Configuration;


namespace Beyond8.AppHost.Extensions
{
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

            var rabbitMq = builder.AddRabbitMQ("rabbitmq")
                .WithContainerName("RabbitMQ")
                .WithImageTag("4.0.2-management-alpine")
                .WithManagementPlugin()
                .WithDataVolume();

            var qdrant = builder.AddQdrant("qdrant")
                .WithContainerName("Qdrant")
                .WithImageTag("dev")
                .WithDataVolume();

            var identityDb = postgres.AddDatabase("identity-db", "Identities");
            var integrationDb = postgres.AddDatabase("integration-db", "Integrations");
            var catalogDb = postgres.AddDatabase("catalog-db", "Catalogs");
            var assessmentDb = postgres.AddDatabase("assessment-db", "Assessments");

            var identityService = builder.AddProject<Projects.Beyond8_Identity_Api>("Identity-Service")
                .WithReference(identityDb)
                .WithReference(redis)
                .WithReference(rabbitMq)
                .WaitFor(postgres)
                .WaitFor(redis)
                .WaitFor(rabbitMq);

            var integrationService = builder.AddProject<Projects.Beyond8_Integration_Api>("Integration-Service")
                .WithReference(integrationDb)
                .WithReference(redis)
                .WithReference(rabbitMq)
                .WithReference(qdrant)
                .WaitFor(postgres)
                .WaitFor(redis)
                .WaitFor(rabbitMq)
                .WaitFor(qdrant);

            var catalogService = builder.AddProject<Projects.Beyond8_Catalog_Api>("Catalog-Service")
                .WithReference(catalogDb)
                .WithReference(redis)
                .WithReference(rabbitMq)
                .WaitFor(postgres)
                .WaitFor(redis)
                .WaitFor(rabbitMq);

            var assessmentService = builder.AddProject<Projects.Beyond8_Assessment_Api>("Assessment-Service")
                .WithReference(assessmentDb)
                .WithReference(redis)
                .WithReference(rabbitMq)
                .WaitFor(postgres)
                .WaitFor(redis)
                .WaitFor(rabbitMq);

            var apiGateway = builder.AddYarp("api-gateway")
                .WithContainerName("ApiGateway")
                .WithHostPort(8080)
                .WithConfiguration(config =>
                {
                    var identityCluster = config.AddProjectCluster(identityService);
                    config.AddRoute("/api/v1/auth/{**catch-all}", identityCluster);
                    config.AddRoute("/api/v1/users/{**catch-all}", identityCluster);
                    config.AddRoute("/api/v1/instructors/{**catch-all}", identityCluster);
                    config.AddRoute("/api/v1/subscriptions/{**catch-all}", identityCluster);

                    var integrationCluster = config.AddProjectCluster(integrationService);
                    config.AddRoute("/api/v1/media/{**catch-all}", integrationCluster);
                    config.AddRoute("/api/v1/ai/{**catch-all}", integrationCluster);
                    config.AddRoute("/api/v1/vnpt-ekyc/{**catch-all}", integrationCluster);
                    config.AddRoute("/api/v1/ai-usage/{**catch-all}", integrationCluster);
                    config.AddRoute("/api/v1/ai-prompts/{**catch-all}", integrationCluster);
                    config.AddRoute("/api/v1/notifications/{**catch-all}", integrationCluster);

                    var catalogCluster = config.AddProjectCluster(catalogService);
                    config.AddRoute("/api/v1/catalog/{**catch-all}", catalogCluster);
                    config.AddRoute("/api/v1/categories/{**catch-all}", catalogCluster);
                    config.AddRoute("/api/v1/courses/{**catch-all}", catalogCluster);
                    config.AddRoute("/api/v1/lessons/{**catch-all}", catalogCluster);

                    var assessmentCluster = config.AddProjectCluster(assessmentService);
                    config.AddRoute("/api/v1/questions/{**catch-all}", assessmentCluster);
                    config.AddRoute("/api/v1/quizzes/{**catch-all}", assessmentCluster);


                    // SignalR hub route
                    config.AddRoute("/hubs/{**catch-all}", integrationCluster);
                });

            var scalarDocs = builder.AddScalarApiReference("api-docs")
               .WithContainerName("ScalarDocs")
               .WithEndpoint("http", endpoint =>
               {
                   endpoint.Port = 8081;
               })
               .WithApiReference(identityService, options => options.AddPreferredSecuritySchemes("Bearer"))
               .WithApiReference(integrationService, options => options.AddPreferredSecuritySchemes("Bearer"))
               .WithApiReference(catalogService, options => options.AddPreferredSecuritySchemes("Bearer"))
               .WithApiReference(assessmentService, options => options.AddPreferredSecuritySchemes("Bearer"));

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
}
