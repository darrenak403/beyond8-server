using Beyond8.Learning.Api.Bootstrapping;
using Beyond8.Learning.Infrastructure.Data;
using Beyond8.Learning.Infrastructure.Data.Seeders;
using Beyond8.DatabaseMigrationHelpers;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

await app.MigrateDbContextAsync<LearningDbContext>(async (_, _) =>
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<LearningDbContext>();
    await LearningSeedData.SeedStudentEnrollmentsAndCertificatesAsync(context);
});

app.UseApplicationServices();
app.Run();
