using Beyond8.DatabaseMigrationHelpers;
using Beyond8.Identity.Api.Bootstrapping;
using Beyond8.Identity.Infrastructure.Data;


var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

await app.MigrateDbContextAsync<IdentityDbContext>();

app.UseApplicationServices();

app.Run();