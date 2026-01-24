using System;
using Beyond8.Catalog.Domain.Repositories.Interfaces;
using Beyond8.Catalog.Infrastructure.Data;
using Beyond8.Catalog.Infrastructure.Repositories.Implements;
using Beyond8.Common.Extensions;
using Beyond8.Common.Utilities;
using Scalar.AspNetCore;

namespace Beyond8.Catalog.Api.Bootstrapping;

public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddOpenApi();
        builder.AddCommonExtensions();
        builder.AddPostgresDatabase<CatalogDbContext>(Const.CatalogServiceDatabase);

        builder.AddServiceRedis(nameof(Catalog), connectionName: Const.Redis);

        builder.AddMassTransitWithRabbitMq();

        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


        return builder;
    }

    public static WebApplication UseApplicationServices(this WebApplication app)
    {
        app.UseCommonService();
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }
        return app;
    }
}
