using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Beyond8.Integration.Infrastructure.Data;

public class IntegrationDbContextFactory : IDesignTimeDbContextFactory<IntegrationDbContext>
{
    public IntegrationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<IntegrationDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=Integration;Username=postgres;Password=postgres",
            options => options.MigrationsAssembly(typeof(IntegrationDbContext).Assembly.FullName));
        return new IntegrationDbContext(optionsBuilder.Options);
    }
}
