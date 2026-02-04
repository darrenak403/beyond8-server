using Beyond8.Common.Extensions;
using Beyond8.Common.Utilities;
using Beyond8.Sale.Application.Dtos.Orders;
using Beyond8.Sale.Application.Services.Interfaces;
using Beyond8.Sale.Domain.Repositories.Interfaces;
using Beyond8.Sale.Infrastructure.Data;
using Beyond8.Sale.Infrastructure.Repositories.Implements;
using FluentValidation;
using Scalar.AspNetCore;

namespace Beyond8.Sale.Api.Bootstrapping;

public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddOpenApi();
        builder.AddCommonExtensions();
        builder.AddPostgresDatabase<SaleDbContext>(Const.SaleServiceDatabase);

        builder.AddServiceRedis(nameof(Sale), connectionName: Const.Redis);

        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Configure MassTransit with RabbitMQ
        builder.AddMassTransitWithRabbitMq(config =>
        {
            // Register consumers for Sale events
            // TODO: Add consumers when needed
        });

        // Register services
        // TODO: Uncomment when service implementations are created
        // builder.Services.AddScoped<IOrderService, OrderService>();
        // builder.Services.AddScoped<IPaymentService, PaymentService>();
        // builder.Services.AddScoped<ICouponService, CouponService>();
        // builder.Services.AddScoped<IInstructorWalletService, InstructorWalletService>();
        // builder.Services.AddScoped<IPayoutService, PayoutService>();
        // builder.Services.AddScoped<ITransactionService, TransactionService>();

        // Add FluentValidation validators
        builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderRequest>();

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

        // TODO: Map API endpoints when implemented
        // app.MapOrderApi();
        // app.MapPaymentApi();
        // app.MapCouponApi();
        // app.MapWalletApi();
        // app.MapPayoutApi();
        // app.MapTransactionApi();

        return app;
    }
}
