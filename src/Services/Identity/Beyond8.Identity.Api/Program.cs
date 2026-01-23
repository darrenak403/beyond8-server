using Beyond8.Identity.Api.Bootstrapping;
using Beyond8.DatabaseMigrationHelpers;
using Beyond8.Identity.Infrastructure.Data;
using Beyond8.Identity.Infrastructure.Data.Seeds;


var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

await app.MigrateDbContextAsync<IdentityDbContext>(async (database, cancellationToken) =>
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    await UserWithRoleSeedData.SeedUserWithRoleAsync(context);
});

app.UseApplicationServices();

app.Run();