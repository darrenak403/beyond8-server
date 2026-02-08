using Beyond8.Sale.Api.Bootstrapping;
using Beyond8.Sale.Infrastructure.Data;
using Beyond8.DatabaseMigrationHelpers;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

await app.MigrateDbContextAsync<SaleDbContext>();

app.UseApplicationServices();
app.Run();
