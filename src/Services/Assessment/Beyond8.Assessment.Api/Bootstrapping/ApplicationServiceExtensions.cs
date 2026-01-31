using Beyond8.Assessment.Api.Apis;
using Beyond8.Assessment.Application.Dtos.Questions;
using Beyond8.Assessment.Application.Services.Interfaces;
using Beyond8.Assessment.Application.Services.Implements;
using Beyond8.Assessment.Domain.Repositories.Interfaces;
using Beyond8.Assessment.Infrastructure.Data;
using Beyond8.Assessment.Infrastructure.Repositories.Implements;
using Beyond8.Common.Extensions;
using Beyond8.Common.Utilities;
using FluentValidation;

namespace Beyond8.Assessment.Api.Bootstrapping
{
    public static class ApplicationServiceExtensions
    {
        public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
        {
            builder.Services.AddOpenApi();
            builder.Services.AddValidatorsFromAssemblyContaining<QuestionRequest>();
            builder.AddCommonExtensions();
            builder.AddPostgresDatabase<AssessmentDbContext>(Const.AssessmentServiceDatabase);
            builder.AddServiceRedis(nameof(Assessment), connectionName: Const.Redis);
            builder.AddMassTransitWithRabbitMq();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            builder.Services.AddScoped<IQuestionService, QuestionService>();
            builder.Services.AddScoped<IQuizService, QuizService>();

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
            app.MapQuestionApi();
            app.MapQuizApi();
            return app;
        }
    }
}