using Beyond8.Common.Extensions;
using Beyond8.Common.Utilities;
using Beyond8.Sale.Api.Apis;
using Beyond8.Sale.Application.Clients.Catalog;
using Beyond8.Sale.Application.Clients.Identity;
using Beyond8.Sale.Application.Consumers.Catalog;
using Beyond8.Sale.Application.Consumers.Identity;
using Beyond8.Sale.Application.Consumers.Learning;
using Beyond8.Sale.Application.Dtos.Orders;
using Beyond8.Sale.Application.Helpers;
using Beyond8.Sale.Application.Services.Implements;
using Beyond8.Sale.Application.Services.Interfaces;
using Beyond8.Sale.Infrastructure.ExternalServices;
using Beyond8.Sale.Domain.Repositories.Interfaces;
using Beyond8.Sale.Infrastructure.Data;
using Beyond8.Sale.Infrastructure.Repositories.Implements;
using FluentValidation;
using Microsoft.Extensions.Hosting;
using Hangfire;
using Polly;
using Polly.Extensions.Http;
using Scalar.AspNetCore;
using Beyond8.Sale.Application.Dtos.Coupons;

namespace Beyond8.Sale.Api.Bootstrapping;

public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddOpenApi();
        builder.AddCommonExtensions();
        builder.AddPostgresDatabase<SaleDbContext>(Const.SaleServiceDatabase);

        builder.AddServiceRedis(nameof(Sale), connectionName: Const.Redis);

        // Configure Hangfire (shared extension from Common)
        builder.AddHangfire(Const.HangfireDatabase);

        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        builder.AddMassTransitWithRabbitMq(config =>
        {
            config.AddConsumer<FreeEnrollmentOrderRequestEventConsumer>();
            config.AddConsumer<CourseUpdatedMetadataEventConsumer>();
            config.AddConsumer<InstructorApprovalEventConsumer>();
        }, queueNamePrefix: "sale");

        // VNPay configuration
        builder.Services.Configure<VNPaySettings>(builder.Configuration.GetSection(VNPaySettings.SectionName));
        builder.Services.AddSingleton<IVNPayService, VNPayService>();

        // Register services
        builder.Services.AddScoped<IOrderService, OrderService>();
        builder.Services.AddScoped<ICouponService, CouponService>();
        builder.Services.AddScoped<ICouponUsageService, CouponUsageService>();
        builder.Services.AddScoped<ICartService, CartService>();
        builder.Services.AddScoped<IPaymentService, PaymentService>();
        builder.Services.AddScoped<ISettlementService, SettlementService>();
        builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderRequest>();

        // Register HTTP clients with Polly retry policy
        var httpPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(r => r.StatusCode == System.Net.HttpStatusCode.NotFound)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt =>
                    TimeSpan.FromMilliseconds(Math.Pow(2, retryAttempt) * 100));

        builder.AddClientServices(httpPolicy);

        // Wallet, Transaction services
        builder.Services.AddScoped<IInstructorWalletService, InstructorWalletService>();
        builder.Services.AddScoped<IPlatformWalletService, PlatformWalletService>();
        builder.Services.AddScoped<ITransactionService, TransactionService>();

        // Payment cleanup will be executed via Hangfire by calling IPaymentService.RunCleanupAsync()

        // Background services: run via Hangfire recurring jobs (do not register as hosted services)


        return builder;
    }

    public static IHostApplicationBuilder AddClientServices(this IHostApplicationBuilder builder, IAsyncPolicy<HttpResponseMessage> httpPolicy)
    {
        builder.Services.AddHttpContextAccessor();

        var catalogBaseUrl = builder.Configuration["Clients:Catalog:BaseUrl"]
                            ?? throw new ArgumentNullException("Catalog Service URL missing");

        builder.Services
            .AddHttpClient<ICatalogClient, CatalogClient>(client =>
            {
                client.BaseAddress = new Uri(catalogBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddPolicyHandler(httpPolicy);

        var identityBaseUrl = builder.Configuration["Clients:Identity:BaseUrl"]
                            ?? throw new ArgumentNullException("Identity Service URL missing");

        builder.Services
            .AddHttpClient<IIdentityClient, IdentityClient>(client =>
            {
                client.BaseAddress = new Uri(identityBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddPolicyHandler(httpPolicy);

        return builder;
    }

    public static WebApplication UseApplicationServices(this WebApplication app)
    {
        app.UseCommonService();

        // Hangfire dashboard + recurring jobs
        app.UseHangfireDashboard("/hangfire", allowAnonymousInDevelopment: false);
        RegisterHangfireRecurringJobs(app);

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.UseHttpsRedirection();

        // Map API endpoints
        app.MapOrderApi();
        app.MapCartApi();
        app.MapPaymentApi();
        app.MapCouponApi();
        app.MapCouponUsageApi();
        app.MapWalletApi();
        app.MapPlatformWalletApi();
        app.MapTransactionApi();
        app.MapSettlementApi();

        return app;
    }

    private static void RegisterHangfireRecurringJobs(WebApplication app)
    {
        // Schedule payment cleanup every 5 minutes
        RecurringJob.AddOrUpdate<IPaymentService>(
            "sale:payment.cleanup:expired-payments",
            x => x.RunCleanupAsync(),
            Cron.MinuteInterval(5));

        // Schedule settlement processing hourly
        RecurringJob.AddOrUpdate<ISettlementService>(
            "sale:settlement.process:hourly",
            x => x.ProcessPendingSettlementsAsync(),
            Cron.Hourly);
    }
}
