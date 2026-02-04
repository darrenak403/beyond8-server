using Beyond8.Common.Extensions;
using Beyond8.Common.Utilities;
using Beyond8.Identity.Api.Apis;
using Beyond8.Identity.Application.Consumers.Catalog;
using Beyond8.Identity.Application.Consumers.Learning;
using Beyond8.Identity.Application.Dtos.Auth;
using Beyond8.Identity.Application.Services.Implements;
using Beyond8.Identity.Application.Services.Interfaces;
using Beyond8.Identity.Domain.Entities;
using Beyond8.Identity.Domain.Repositories.Interfaces;
using Beyond8.Identity.Infrastructure.Data;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Scalar.AspNetCore;

namespace Beyond8.Identity.Api.Bootstrapping
{
    public static class Bootstrapper
    {
        public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
        {

            builder.Services.AddOpenApi();

            builder.AddCommonExtensions();

            builder.AddPostgresDatabase<IdentityDbContext>(Const.IdentityServiceDatabase);

            builder.AddServiceRedis(nameof(Identity), connectionName: Const.Redis);

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Configure MassTransit with RabbitMQ
            builder.AddMassTransitWithRabbitMq(config =>
            {
                // Register consumers from Catalog events
                config.AddConsumer<CoursePublishedEventConsumer>();
                config.AddConsumer<CourseUnpublishedEventConsumer>();
                config.AddConsumer<CourseEnrollmentCountChangedEventConsumer>();
            });

            // Register services
            builder.Services.AddScoped<PasswordHasher<User>>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IInstructorService, InstructorService>();
            builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();

            // Add FluentValidation validators
            builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequest>();

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

            app.MapAuthApi();
            app.MapUserApi();
            app.MapInstructorApi();
            app.MapSubscriptionApi();

            return app;
        }
    }
}
