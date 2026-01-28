using Beyond8.Catalog.Api.Apis;
using Beyond8.Catalog.Application.Clients.Identity;
using Beyond8.Catalog.Application.Dtos.Categories;
using Beyond8.Catalog.Application.Dtos.Courses;
using Beyond8.Catalog.Application.Services.Implements;
using Beyond8.Catalog.Application.Services.Interfaces;
using Beyond8.Catalog.Domain.Repositories.Interfaces;
using Beyond8.Catalog.Infrastructure.Data;
using Beyond8.Catalog.Infrastructure.Repositories.Implements;
using Beyond8.Common.Extensions;
using Beyond8.Common.Utilities;
using FluentValidation;
using Polly;
using Polly.Extensions.Http;
using Scalar.AspNetCore;

namespace Beyond8.Catalog.Api.Bootstrapping
{
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
            builder.Services.AddScoped<ICourseService, CourseService>();

            builder.Services.AddValidatorsFromAssemblyContaining<CreateCategoryRequest>();
            return builder;
        }

        public static IHostApplicationBuilder AddClientServices(this IHostApplicationBuilder builder)
        {
            builder.Services.AddHttpContextAccessor();

            var identityBaseUrl = builder.Configuration.GetSection("Clients:Identity:BaseUrl") ?? throw new ArgumentNullException("Clients:Identity:BaseUrl configuration is missing");
            builder.Services.AddHttpClient<IIdentityClient, IdentityClient>(client =>
            {
                client.BaseAddress = new Uri(identityBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddPolicyHandler(GetResiliencePolicy());

            return builder;
        }

        private static IAsyncPolicy<HttpResponseMessage> GetResiliencePolicy()
        {
            var jitterer = new Random();

            var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt =>
                        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                        + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000))
                );

            var circuitBreakerPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(30)
                );

            return Policy.WrapAsync(retryPolicy, circuitBreakerPolicy);
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
}
