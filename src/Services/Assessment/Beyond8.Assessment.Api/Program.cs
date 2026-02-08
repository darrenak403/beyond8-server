using Beyond8.Assessment.Api.Bootstrapping;
using Beyond8.Assessment.Infrastructure.Data;
using Beyond8.Assessment.Infrastructure.Data.Seeders;
using Beyond8.DatabaseMigrationHelpers;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

await app.MigrateDbContextAsync<AssessmentDbContext>(async (_, _) =>
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AssessmentDbContext>();
    await AssessmentSeedData.SeedQuizzesAndQuestionsAsync(context);
    await AssessmentSeedData.SeedAssignmentsAsync(context);
});

app.UseApplicationServices();

app.Run();
