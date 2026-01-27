using Beyond8.Catalog.Api.Apis;
using Beyond8.Catalog.Application.Dtos.Categories;
using Beyond8.Catalog.Application.Services.Implements;
using Beyond8.Catalog.Application.Services.Interfaces;
using Beyond8.Catalog.Domain.Repositories.Interfaces;
using Beyond8.Catalog.Infrastructure.Data;
using Beyond8.Catalog.Infrastructure.Repositories.Implements;
using Beyond8.Common.Extensions;
using Beyond8.Common.Utilities;
using FluentValidation;
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

        builder.Services.AddScoped<ICategoryService, CategoryService>();

        builder.Services.AddValidatorsFromAssemblyContaining<CreateCategoryRequest>();
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
        app.UseHttpsRedirection();

        app.MapCategoryApi();

        return app;
    }
}
