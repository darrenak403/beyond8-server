using Beyond8.Common.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Beyond8.Common.Extensions
{
    public static class JwtExtensions
    {
        private const string UnauthorizedError = "Unauthorized";
        private const string UnauthorizedMessage = "You are not authorized to access this resource";
        private const string TokenExpiredHeader = "Token-Expired";

        public static IHostApplicationBuilder AddJwtAuthentication(this IHostApplicationBuilder builder)
        {
            var jwtOptions = GetJwtOptions(builder.Configuration);
            ValidateJwtOptions(jwtOptions);

            ConfigureServices(builder.Services, builder.Configuration, jwtOptions);

            return builder;
        }

        private static JwtBearerConfigurationOptions GetJwtOptions(IConfiguration configuration)
        {
            return configuration
                .GetSection(JwtBearerConfigurationOptions.SectionName)
                .Get<JwtBearerConfigurationOptions>()
                ?? throw new InvalidOperationException(
                    $"JWT configuration section '{JwtBearerConfigurationOptions.SectionName}' not found in appsettings.json");
        }

        private static void ValidateJwtOptions(JwtBearerConfigurationOptions jwtOptions)
        {
            if (string.IsNullOrWhiteSpace(jwtOptions.SecretKey))
            {
                throw new InvalidOperationException("JWT SecretKey cannot be null or empty");
            }
        }

        private static void ConfigureServices(
            IServiceCollection services,
            IConfiguration configuration,
            JwtBearerConfigurationOptions jwtOptions)
        {
            services.Configure<JwtBearerConfigurationOptions>(
                configuration.GetSection(JwtBearerConfigurationOptions.SectionName));

            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddHttpContextAccessor();

            services.AddAuthentication(ConfigureAuthenticationOptions)
                    .AddJwtBearer(options => ConfigureJwtBearerOptions(options, jwtOptions));

            services.AddAuthorization();
        }

        private static void ConfigureAuthenticationOptions(Microsoft.AspNetCore.Authentication.AuthenticationOptions options)
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }

        private static void ConfigureJwtBearerOptions(JwtBearerOptions options, JwtBearerConfigurationOptions jwtOptions)
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = CreateTokenValidationParameters(jwtOptions);
            options.Events = CreateJwtBearerEvents();

            // Configure SignalR to read token from query string
            options.Events.OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                // If the request is for SignalR hub, read token from query string
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            };
        }

        private static TokenValidationParameters CreateTokenValidationParameters(JwtBearerConfigurationOptions jwtOptions)
        {
            var key = Encoding.UTF8.GetBytes(jwtOptions.SecretKey);

            return new TokenValidationParameters
            {
                ValidateIssuer = jwtOptions.ValidateIssuer,
                ValidateAudience = jwtOptions.ValidateAudience,
                ValidateLifetime = jwtOptions.ValidateLifetime,
                ValidateIssuerSigningKey = jwtOptions.ValidateIssuerSigningKey,
                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.FromSeconds(jwtOptions.ClockSkewSeconds)
            };
        }

        private static JwtBearerEvents CreateJwtBearerEvents()
        {
            return new JwtBearerEvents
            {
                OnAuthenticationFailed = HandleAuthenticationFailed,
                OnChallenge = HandleChallenge
            };
        }

        private static Task HandleAuthenticationFailed(AuthenticationFailedContext context)
        {
            if (context.Exception is SecurityTokenExpiredException)
            {
                context.Response.Headers.Append(TokenExpiredHeader, "true");
            }
            return Task.CompletedTask;
        }

        private static Task HandleChallenge(JwtBearerChallengeContext context)
        {
            context.HandleResponse();
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            var errorResponse = new
            {
                error = UnauthorizedError,
                message = UnauthorizedMessage
            };

            return context.Response.WriteAsJsonAsync(errorResponse);
        }
    }
}