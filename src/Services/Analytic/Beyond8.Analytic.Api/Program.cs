using Beyond8.Analytic.Api.Bootstrapping;
using Beyond8.Analytic.Infrastructure.Data;
using Beyond8.DatabaseMigrationHelpers;

var builder = WebApplication.CreateBuilder(args);
builder.AddApplicationServices();

var app = builder.Build();
await app.MigrateDbContextAsync<AnalyticDbContext>();

app.UseApplicationServices();
app.Run();
