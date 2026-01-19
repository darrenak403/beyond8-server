using System;
using Beyond8.Common.Extensions;
using Beyond8.Common.Utilities;
using Beyond8.Identity.Api.Apis;
using Beyond8.Identity.Application.Services.Implements;
using Beyond8.Identity.Application.Services.Interfaces;
using Beyond8.Identity.Domain.Entities;
using Beyond8.Identity.Domain.Repositories.Interfaces;
using Beyond8.Identity.Infrastructure.Data;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace Beyond8.Identity.Api.Bootstrapping;

public static class Bootstrapper
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {

        builder.Services.AddOpenApi();

        builder.AddCommonExtensions();

        builder.AddPostgresDatabase<IdentityDbContext>(Const.IdentityServiceDatabase);

        builder.AddServiceRedis(nameof(Identity), connectionName: Const.Redis);

        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        builder.Services.AddScoped<PasswordHasher<User>>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IInstructorService, InstructorService>();

        // Add FluentValidation validators
        builder.Services.AddValidatorsFromAssemblyContaining<Beyond8.Identity.Application.Dtos.Auth.RegisterRequest>();

        return builder;
    }

    public static WebApplication UseApplicationServices(this WebApplication app)
    {
        app.UseCommonService();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.MapAuthApi();
        app.MapUserApi();
        app.MapInstructorApi();
        return app;
    }
}
