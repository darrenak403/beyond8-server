using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace Beyond8.Common.Extensions;

public static class DatabaseExtensions
{
    public static IHostApplicationBuilder AddPostgresDatabase<TContext>(
        this IHostApplicationBuilder builder,
        string databaseName) where TContext : DbContext
    {
        builder.AddNpgsqlDbContext<TContext>(databaseName, configureDbContextOptions: options =>
        {
            options.UseNpgsql(builder => builder.MigrationsAssembly(typeof(TContext).Assembly.FullName));
        });

        return builder;
    }
}
