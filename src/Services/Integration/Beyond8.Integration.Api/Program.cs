using Beyond8.DatabaseMigrationHelpers;
using Beyond8.Integration.Api.Bootstrapping;
using Beyond8.Integration.Infrastructure.Data;
using Beyond8.Integration.Infrastructure.Data.Seeders;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

await app.MigrateDbContextAsync<IntegrationDbContext>();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<IntegrationDbContext>();
    await AiPromptSeeder.SeedAsync(context);
}

app.UseApplicationServices();

app.Run();
