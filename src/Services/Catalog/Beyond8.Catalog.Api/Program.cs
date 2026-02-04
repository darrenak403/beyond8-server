using Beyond8.Catalog.Infrastructure.Data;
using Beyond8.Catalog.Infrastructure.Data.Seeders;
using Beyond8.Catalog.Api.Bootstrapping;
using Beyond8.DatabaseMigrationHelpers;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

await app.MigrateDbContextAsync<CatalogDbContext>(async (database, cancellationToken) =>
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

    await context.Database.ExecuteSqlRawAsync(@"ALTER TABLE ""Courses"" DROP COLUMN IF EXISTS ""TotalSections"";");
    await context.Database.ExecuteSqlRawAsync(@"ALTER TABLE ""Courses"" DROP COLUMN IF EXISTS ""TotalLessons"";");
    await context.Database.ExecuteSqlRawAsync(@"ALTER TABLE ""Courses"" DROP COLUMN IF EXISTS ""TotalDurationMinutes"";");

    await CatalogSeedData.SeedCategoriesAsync(context);
    await CatalogSeedData.SeedCoursesAsync(context);
});

app.UseApplicationServices();
app.Run();