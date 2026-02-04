using Beyond8.Assessment.Api.Apis;
using Beyond8.Assessment.Application.Consumers.Catalog;
using Beyond8.Assessment.Application.Consumers.Integration;
using Beyond8.Assessment.Application.Dtos.Questions;
using Beyond8.Assessment.Application.Services.Interfaces;
using Beyond8.Assessment.Application.Services.Implements;
using Beyond8.Assessment.Domain.Repositories.Interfaces;
using Beyond8.Assessment.Infrastructure.Data;
using Beyond8.Assessment.Infrastructure.Repositories.Implements;
using Beyond8.Common.Extensions;
using Beyond8.Common.Utilities;
using FluentValidation;
using Beyond8.Assessment.Application.Clients.Catalog;
using Beyond8.Assessment.Application.Clients.Learning;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Polly;
using Polly.Extensions.Http;

namespace Beyond8.Assessment.Api.Bootstrapping
{
    public static class ApplicationServiceExtensions
    {
        public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
        {
            builder.Services.AddOpenApi();
            builder.Services.AddValidatorsFromAssemblyContaining<QuestionRequest>();
            builder.AddCommonExtensions();
            builder.AddPostgresDatabase<AssessmentDbContext>(Const.AssessmentServiceDatabase, options =>
                options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));
            builder.AddServiceRedis(nameof(Assessment), connectionName: Const.Redis);
            builder.AddMassTransitWithRabbitMq(config =>
            {
                // Catalog events
                config.AddConsumer<LessonQuizUnlinkedEventConsumer>();

                // Integration events (AI Grading)
                config.AddConsumer<AiGradingCompletedConsumer>();
            });
            builder.AddClientServices();

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            builder.Services.AddScoped<IQuestionService, QuestionService>();
            builder.Services.AddScoped<IQuizService, QuizService>();
            builder.Services.AddScoped<IQuizAttemptService, QuizAttemptService>();
            builder.Services.AddScoped<IAssignmentService, AssignmentService>();
            builder.Services.AddScoped<IAssignmentSubmissionService, AssignmentSubmissionService>();

            return builder;
        }

        private static IHostApplicationBuilder AddClientServices(this IHostApplicationBuilder builder)
        {
            var catalogBaseUrl = builder.Configuration["Clients:Catalog:BaseUrl"]
                                 ?? throw new ArgumentNullException("Catalog URL missing");
            var learningBaseUrl = builder.Configuration["Clients:Learning:BaseUrl"]
                                 ?? throw new ArgumentNullException("Learning URL missing");

            builder.Services.AddHttpClient<ICatalogService, CatalogService>(client =>
            {
                client.BaseAddress = new Uri(catalogBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddPolicyHandler(GetResiliencePolicy());

            builder.Services.AddHttpClient<ILearningClient, LearningClient>(client =>
            {
                client.BaseAddress = new Uri(learningBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddPolicyHandler(GetResiliencePolicy());

            builder.Services.AddHttpContextAccessor();
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
            }
            app.UseHttpsRedirection();
            app.MapQuestionApi();
            app.MapQuizApi();
            app.MapQuizAttemptApi();
            app.MapAssignmentApi();

            return app;
        }
    }
}