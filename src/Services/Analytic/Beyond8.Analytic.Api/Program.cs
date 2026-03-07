using Beyond8.Analytic.Api.Bootstrapping;
using Beyond8.Analytic.Infrastructure.Data;
using Beyond8.Analytic.Infrastructure.Data.Seeders;
using Beyond8.DatabaseMigrationHelpers;

var builder = WebApplication.CreateBuilder(args);
builder.AddApplicationServices();

var app = builder.Build();
await app.MigrateDbContextAsync<AnalyticDbContext>(async (_, _) =>
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AnalyticDbContext>();
    await AnalyticSeedData.SeedAsync(context);
});

app.UseApplicationServices();
app.Run();
