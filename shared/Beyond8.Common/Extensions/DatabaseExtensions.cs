using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace Beyond8.Common.Extensions
{
    public static class DatabaseExtensions
    {
        public static IHostApplicationBuilder AddPostgresDatabase<TContext>(
            this IHostApplicationBuilder builder,
            string databaseName,
            Action<DbContextOptionsBuilder>? configureOptions = null) where TContext : DbContext
        {
            builder.AddNpgsqlDbContext<TContext>(databaseName, configureDbContextOptions: options =>
            {
                options.UseNpgsql(npgsql => npgsql.MigrationsAssembly(typeof(TContext).Assembly.FullName));
                configureOptions?.Invoke(options);
            });

            return builder;
        }
    }
}
