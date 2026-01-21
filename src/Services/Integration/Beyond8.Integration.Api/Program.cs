using Beyond8.Integration.Api.Bootstrapping;
using Beyond8.DatabaseMigrationHelpers;
using Beyond8.Integration.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

await app.MigrateDbContextAsync<IntegrationDbContext>();

app.UseApplicationServices();

app.Run();
