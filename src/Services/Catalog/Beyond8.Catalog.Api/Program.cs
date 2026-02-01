using Beyond8.Catalog.Infrastructure.Data;
using Beyond8.Catalog.Infrastructure.Data.Seeders;
using Beyond8.Catalog.Api.Bootstrapping;
using Beyond8.DatabaseMigrationHelpers;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

await app.MigrateDbContextAsync<CatalogDbContext>();

// Seed data after migration
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    await CatalogSeedData.SeedCategoriesAsync(context);
}

app.UseApplicationServices();

app.Run();
