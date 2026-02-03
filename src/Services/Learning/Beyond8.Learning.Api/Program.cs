using Beyond8.Learning.Api.Bootstrapping;
using Beyond8.Learning.Infrastructure.Data;
using Beyond8.DatabaseMigrationHelpers;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

await app.MigrateDbContextAsync<LearningDbContext>();

app.UseApplicationServices();
app.Run();
