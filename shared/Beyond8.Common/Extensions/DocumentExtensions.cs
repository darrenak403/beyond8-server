using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;

namespace Beyond8.Common.Extensions;

public static class DocumentExtensions
{
    public static IHostApplicationBuilder AddDocumentGlobalAuthentication(this IHostApplicationBuilder builder)
    {
        builder.Services.AddOpenApi(options =>
        {
            _ = options.AddDocumentTransformer(static (document, context, cancellationToken) =>
            {
                document.Info = new OpenApiInfo
                {
                    Title = "Beyond8 API",
                    Version = "v1",
                    Description = "Beyond8 API Documentation"
                };

                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

                document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Nhập token JWT",
                    In = ParameterLocation.Header,
                    Name = "Authorization"
                };

                return Task.CompletedTask;
            });

            _ = options.AddOperationTransformer((operation, context, cancellationToken) =>
            {
                var metadata = context.Description?.ActionDescriptor?.EndpointMetadata ?? System.Array.Empty<object>();
                var allowAnonymous = metadata.OfType<IAllowAnonymous>().Any();
                var requiresAuth = metadata.OfType<IAuthorizeData>().Any();

                if (requiresAuth && !allowAnonymous)
                {
                    operation.Security ??= [];
                    operation.Security.Add(new OpenApiSecurityRequirement
                    {
                        { new OpenApiSecuritySchemeReference("Bearer"), new List<string>() }
                    });
                }

                return Task.CompletedTask;
            });
        });

        return builder;
    }
}