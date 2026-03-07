using Beyond8.Sale.Api.Bootstrapping;
using Beyond8.Sale.Infrastructure.Data;
using Beyond8.Sale.Infrastructure.Data.Seeders;
using Beyond8.DatabaseMigrationHelpers;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

await app.MigrateDbContextAsync<SaleDbContext>(async (database, cancellationToken) =>
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<SaleDbContext>();
    await SaleSeedData.SeedCouponsAsync(context);
    await SaleSeedData.SeedWalletsAsync(context);
});

app.UseApplicationServices();
app.Run();
