using Beyond8.Assessment.Api.Bootstrapping;
using Beyond8.Assessment.Infrastructure.Data;
using Beyond8.DatabaseMigrationHelpers;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

await app.MigrateDbContextAsync<AssessmentDbContext>();

app.UseApplicationServices();

app.Run();
