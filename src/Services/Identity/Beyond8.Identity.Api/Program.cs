using Beyond8.Identity.Api.Bootstrapping;
using Beyond8.DatabaseMigrationHelpers;
using Beyond8.Identity.Infrastructure.Data;
using Beyond8.Identity.Infrastructure.Data.Seeders;


var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

await app.MigrateDbContextAsync<IdentityDbContext>(async (database, cancellationToken) =>
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    await UserWithRoleSeedData.SeedUserWithRoleAsync(context);
    await SubscriptionPlanSeedData.SeedSubscriptionPlansAsync(context);
});

app.UseApplicationServices();

app.Run();