using Beyond8.Common.Extensions;
using Beyond8.Common.Utilities;
using Beyond8.Sale.Domain.Repositories.Interfaces;
using Beyond8.Sale.Infrastructure.Data;
using Beyond8.Sale.Infrastructure.Repositories.Implements;
using Scalar.AspNetCore;

namespace Beyond8.Sale.Api.Bootstrapping;

public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddOpenApi();
        builder.AddCommonExtensions();
        builder.AddPostgresDatabase<SaleDbContext>(Const.SaleServiceDatabase);

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
        app.UseHttpsRedirection();

        return app;
    }
}
